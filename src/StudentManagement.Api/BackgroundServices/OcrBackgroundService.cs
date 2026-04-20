using System.Threading.Channels;
using StudentManagement.Api.Models;
using StudentManagement.Application.Interfaces;

namespace StudentManagement.Api.BackgroundServices;

public sealed class OcrBackgroundService : BackgroundService
{
    private readonly Channel<OcrJob> _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OcrBackgroundService> _logger;

    public OcrBackgroundService(
        Channel<OcrJob> channel,
        IServiceProvider serviceProvider,
        ILogger<OcrBackgroundService> logger)
    {
        _channel = channel;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var notifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotificationService>();
                var agentClient = scope.ServiceProvider.GetRequiredService<IAgentClient>();

                await notifier.SendOcrProgressAsync(job.SessionId, "Analiz başlıyor...", 10, stoppingToken);

                await using var fileStream = File.OpenRead(job.TempFilePath);
                await notifier.SendOcrProgressAsync(job.SessionId, "Agent'a gönderiliyor...", 30, stoppingToken);

                var resultJson = await agentClient.ChatWithDocumentAsync(
                    job.Message,
                    fileStream,
                    job.OriginalFileName,
                    "application/octet-stream",
                    stoppingToken);

                await notifier.SendOcrProgressAsync(job.SessionId, "İşlendi.", 90, stoppingToken);
                await notifier.SendOcrCompletedAsync(job.SessionId, resultJson, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "OCR job {JobId} başarısız oldu", job.JobId);
                try
                {
                    await using var errScope = _serviceProvider.CreateAsyncScope();
                    var notifier = errScope.ServiceProvider.GetRequiredService<IRealtimeNotificationService>();
                    await notifier.SendOcrFailedAsync(job.SessionId, ex.Message, stoppingToken);
                }
                catch (Exception notifyEx)
                {
                    _logger.LogWarning(notifyEx, "Hata bildirimi gönderilemedi. Session: {SessionId}", job.SessionId);
                }
            }
            finally
            {
                if (File.Exists(job.TempFilePath))
                    File.Delete(job.TempFilePath);
            }
        }
    }
}
