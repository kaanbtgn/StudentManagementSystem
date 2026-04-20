using System.Collections.Concurrent;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using StudentManagement.Agent.Services.Models;
using StudentManagement.Agent.Services.Prompts;

namespace StudentManagement.Agent.Services;

public sealed class StudentManagementAgent
{
    private readonly IChatClient? _chat;
    private readonly AzureDocumentIntelligenceService _ocr;
    private readonly Lazy<Task<McpClient>> _mcpClientFactory;
    private readonly ILogger<StudentManagementAgent> _logger;

    // Session bazlı in-memory geçmiş — sonraki fazda Redis'e taşınacak
    private static readonly ConcurrentDictionary<string, List<ChatMessage>> _histories = new();
    private static readonly SemaphoreSlim _toolSemaphore = new(1, 1);
    private static IList<McpClientTool>? _cachedTools;

    private const int MaxHistorySessions = 100;
    private const int MaxMessagesPerSession = 20;

    public StudentManagementAgent(
        AzureDocumentIntelligenceService ocr,
        Lazy<Task<McpClient>> mcpClientFactory,
        ILogger<StudentManagementAgent> logger,
        IChatClient? chat = null)
    {
        _chat = chat;
        _ocr = ocr;
        _mcpClientFactory = mcpClientFactory;
        _logger = logger;
    }

    public async Task<AgentResponse> ProcessAsync(AgentRequest request, CancellationToken ct)
    {
        // 1. Dosya varsa OCR çalıştır
        string? ocrContent = null;
        OcrMetadata? ocrMetadata = null;

        if (request.File is not null)
        {
            _logger.LogInformation("OCR başlatılıyor: {FileName}", request.File.FileName);
            await using var stream = request.File.OpenReadStream();
            var ocrResult = await _ocr.ParseDocumentAsync(stream, request.File.FileName, ct);
            ocrContent = ocrResult.RawContent;
            ocrMetadata = new OcrMetadata(ocrResult.OverallConfidence, ocrResult.RequiresHumanReview);

            _logger.LogInformation(
                "OCR tamamlandı. Güven: {Confidence:P1}, İnsan onayı: {Review}",
                ocrResult.OverallConfidence, ocrResult.RequiresHumanReview);
        }

        // 2. MCP araçlarını al (ilk çağrıda yükle, sonraki çağrılarda önbellekten döner)
        var tools = await GetCachedToolsAsync(ct);

        // 3. Konuşma geçmişini al veya oluştur
        EnsureSessionCapacity();
        var history = _histories.GetOrAdd(request.SessionId, _ => new List<ChatMessage>());

        if (history.Count == 0)
            history.Add(new ChatMessage(ChatRole.System, SystemPrompt.Text));

        // OCR içeriği varsa kullanıcı mesajına ekle
        var userText = ocrContent is not null
            ? $"{request.Message}\n\n[Belge İçeriği (OCR)]:\n{ocrContent}"
            : request.Message;

        history.Add(new ChatMessage(ChatRole.User, userText));

        // 4. LLM'e gönder — FunctionInvocationMiddleware tool seçimi ve çağrımı otomatik yapar
        if (_chat is null)
            return new AgentResponse(Reply: "Azure OpenAI yapılandırılmamış. appsettings.Development.json dosyasını kontrol edin.", OcrMetadata: ocrMetadata);

        _logger.LogInformation("LLM çağrısı başlatılıyor. Session: {SessionId}", request.SessionId);

        var options = new ChatOptions
        {
            Tools = [.. tools],
            MaxOutputTokens = 1024,
        };

        var trimmedHistory = TrimHistory(history);
        var completion = await _chat.GetResponseAsync(trimmedHistory, options, ct);

        var reply = completion.Text ?? string.Empty;

        // 5. Yalnızca kullanıcı ve asistan yanıtını geçmişe kaydet (tool mesajları gürültü ekler)
        history.AddMessages(completion);

        _logger.LogInformation("Yanıt üretildi. Session: {SessionId}", request.SessionId);

        return new AgentResponse(Reply: reply, OcrMetadata: ocrMetadata);
    }

    // ── Yardımcı metotlar ────────────────────────────────────────────────

    private async Task<IList<McpClientTool>> GetCachedToolsAsync(CancellationToken ct)
    {
        if (_cachedTools is not null)
            return _cachedTools;

        await _toolSemaphore.WaitAsync(ct);
        try
        {
            if (_cachedTools is null)
            {
                _logger.LogInformation("MCP araçları yükleniyor...");
                var mcpClient = await _mcpClientFactory.Value;
                _cachedTools = await mcpClient.ListToolsAsync(cancellationToken: ct);
                _logger.LogInformation("{Count} MCP aracı yüklendi.", _cachedTools.Count);
            }

            return _cachedTools;
        }
        finally
        {
            _toolSemaphore.Release();
        }
    }

    private static List<ChatMessage> TrimHistory(List<ChatMessage> history)
    {
        if (history.Count <= MaxMessagesPerSession)
            return history;

        // System mesajını koru, en eski user/assistant mesajları at
        var system = history.FirstOrDefault(m => m.Role == ChatRole.System);
        var recent = history
            .Where(m => m.Role != ChatRole.System)
            .TakeLast(MaxMessagesPerSession - 1)
            .ToList();

        if (system is not null)
            recent.Insert(0, system);

        return recent;
    }

    private static void EnsureSessionCapacity()
    {
        if (_histories.Count < MaxHistorySessions)
            return;

        // En eski session'ı bul ve temizle (basit LRU)
        var oldest = _histories.Keys.FirstOrDefault();
        if (oldest is not null)
            _histories.TryRemove(oldest, out _);
    }
}
