using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StudentManagement.Application.Interfaces;

namespace StudentManagement.Infrastructure.ExternalServices;

/// <summary>
/// API → Agent HTTP köprüsü.
///
/// Akış:
///   1. Session history'yi Redis / MongoDB'den yükler (ChatSessionHistoryService).
///   2. History + message'ı Agent'a gönderir — Agent persistence bilmez.
///   3. Agent'ın döndürdüğü reply'dan user + assistant çiftini oluşturur ve kaydeder.
///   4. Frontend'e temiz JSON döner.
/// </summary>
internal sealed class AgentHttpClient : IAgentClient
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ChatSessionHistoryService _historyService;
    private readonly ILogger<AgentHttpClient> _logger;

    public AgentHttpClient(
        HttpClient http,
        IHttpContextAccessor httpContextAccessor,
        ChatSessionHistoryService historyService,
        ILogger<AgentHttpClient> logger)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
        _historyService = historyService;
        _logger = logger;
    }

    public async Task<string> ChatAsync(string message, CancellationToken ct = default)
    {
        _logger.LogInformation("Agent chat isteği gönderiliyor.");

        var sessionId = GetSessionId();
        var history = sessionId.Length > 0
            ? await _historyService.LoadAsync(sessionId, ct)
            : [];

        var payload = JsonSerializer.Serialize(new { message, history }, JsonOpts);
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _http.PostAsync("/api/chat", content, ct);
        response.EnsureSuccessStatusCode();

        var rawJson = await response.Content.ReadAsStringAsync(ct);
        return await ParseAndPersistAsync(sessionId, message, history, rawJson, ct);
    }

    public async Task<string> ChatWithDocumentAsync(
        string message,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Agent'a dosyalı chat isteği gönderiliyor: {FileName}", fileName);

        var sessionId = GetSessionId();
        var history = sessionId.Length > 0
            ? await _historyService.LoadAsync(sessionId, ct)
            : [];

        var historyJson = JsonSerializer.Serialize(history, JsonOpts);

        using var form = new MultipartFormDataContent();

        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", fileName);

        form.Add(new StringContent(message, Encoding.UTF8), "message");
        form.Add(new StringContent(historyJson, Encoding.UTF8), "historyJson");

        HttpResponseMessage response = await _http.PostAsync("/api/chat/document", form, ct);
        response.EnsureSuccessStatusCode();

        var rawJson = await response.Content.ReadAsStringAsync(ct);
        return await ParseAndPersistAsync(sessionId, message, history, rawJson, ct);
    }

    /// <summary>
    /// Agent yanıtından <c>reply</c> alanını okur, user + assistant çifti olarak
    /// MongoDB'ye append eder ve Redis sliding window'u günceller.
    /// </summary>
    private async Task<string> ParseAndPersistAsync(
        string sessionId, string userMessage, List<HistoryEntry> currentHistory, string rawJson, CancellationToken ct)
    {
        var node = JsonNode.Parse(rawJson)?.AsObject();
        if (node is null) return rawJson;

        if (sessionId.Length > 0)
        {
            var reply = node["reply"]?.GetValue<string>() ?? string.Empty;
            if (reply.Length > 0)
            {
                var entries = new List<HistoryEntry>
                {
                    new("user", userMessage),
                    new("assistant", reply),
                };
                await _historyService.AppendAsync(sessionId, entries, currentHistory, ct);
            }
        }

        return node.ToJsonString();
    }

    private string GetSessionId()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx is null) return string.Empty;

        // SessionIdMiddleware tarafından Items'a yazılan değeri al;
        // yoksa X-Session-Id header'ından oku
        if (ctx.Items.TryGetValue("SessionId", out var fromItems) && fromItems is string s && !string.IsNullOrEmpty(s))
            return s;

        return ctx.Request.Headers.TryGetValue("X-Session-Id", out var fromHeader)
            ? fromHeader.ToString()
            : string.Empty;
    }
}
