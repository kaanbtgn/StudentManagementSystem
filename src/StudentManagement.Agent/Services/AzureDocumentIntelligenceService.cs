using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace StudentManagement.Agent.Services;

public sealed class AzureDocumentIntelligenceService
{
    private const double HumanReviewThreshold = 0.85;

    private readonly DocumentIntelligenceClient _client;
    private readonly ILogger<AzureDocumentIntelligenceService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public AzureDocumentIntelligenceService(
        DocumentIntelligenceClient client,
        ILogger<AzureDocumentIntelligenceService> logger)
    {
        _client = client;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<RequestFailedException>(IsTransient)
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, delay, attempt, _) =>
                    _logger.LogWarning(
                        "Azure Document Intelligence geçici hata (deneme {Attempt}/3, bekleme {Delay}s): {Error}",
                        attempt, delay.TotalSeconds, exception.Message));
    }

    public async Task<OcrResult> ParseDocumentAsync(
        Stream fileStream,
        string fileName,
        CancellationToken ct = default)
    {
        _logger.LogInformation("OCR başlatılıyor: {FileName}", fileName);

        BinaryData content = await BinaryData.FromStreamAsync(fileStream, ct);

        AnalyzeResult result = await _retryPolicy.ExecuteAsync(async () =>
        {
            Operation<AnalyzeResult> operation = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-layout",
                content,
                cancellationToken: ct);

            return operation.Value;
        });

        string rawContent = result.Content ?? string.Empty;
        double overallConfidence = CalculateWordConfidence(result);
        bool requiresHumanReview = overallConfidence < HumanReviewThreshold;

        if (requiresHumanReview)
            _logger.LogWarning(
                "Düşük OCR güveni, insan onayı gerekiyor. Dosya: {FileName}, Güven: {Confidence:P1}",
                fileName, overallConfidence);
        else
            _logger.LogInformation(
                "OCR tamamlandı. Dosya: {FileName}, Güven: {Confidence:P1}",
                fileName, overallConfidence);

        return new OcrResult(rawContent, overallConfidence, requiresHumanReview);
    }

    private static double CalculateWordConfidence(AnalyzeResult result)
    {
        var scores = result.Pages
            .SelectMany(p => p.Words)
            .Select(w => w.Confidence)
            .ToList();

        return scores.Count == 0 ? 0.0 : scores.Average();
    }

    private static bool IsTransient(RequestFailedException ex) =>
        ex.Status is 429 or 500 or 502 or 503 or 504;
}


