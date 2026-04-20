using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StudentManagement.Application.Interfaces;

namespace StudentManagement.Infrastructure.ExternalServices;

internal sealed class AgentHttpClient : IAgentClient
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly ILogger<AgentHttpClient> _logger;

    public AgentHttpClient(HttpClient http, ILogger<AgentHttpClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<string> ChatAsync(string message, CancellationToken ct = default)
    {
        _logger.LogInformation("Agent chat isteği gönderiliyor.");

        var payload = JsonSerializer.Serialize(new { message }, JsonOpts);
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _http.PostAsync("/api/chat", content, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(ct);
    }

    public async Task<string> ChatWithDocumentAsync(
        string message,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Agent'a dosyalı chat isteği gönderiliyor: {FileName}", fileName);

        using var form = new MultipartFormDataContent();

        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", fileName);

        var messageContent = new StringContent(message, Encoding.UTF8);
        form.Add(messageContent, "message");

        HttpResponseMessage response = await _http.PostAsync("/api/chat/document", form, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(ct);
    }
}
