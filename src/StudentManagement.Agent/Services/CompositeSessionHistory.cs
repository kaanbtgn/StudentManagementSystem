using Microsoft.Extensions.AI;

namespace StudentManagement.Agent.Services;

/// <summary>
/// Redis + MongoDB dual-write session history.
///
/// Okuma: Redis → hit ise döner; miss ise MongoDB'den son 20 mesaj yükler
///        ve Redis'e geri yazar (warm-up).
/// Yazma: Her ikisine de yazar. Redis hata verse devam eder, MongoDB hata verse devam eder.
/// </summary>
internal sealed class CompositeSessionHistory : ISessionHistory
{
    private readonly RedisSessionHistory _redis;
    private readonly MongoSessionHistory _mongo;
    private readonly ILogger<CompositeSessionHistory> _logger;

    public CompositeSessionHistory(
        RedisSessionHistory redis,
        MongoSessionHistory mongo,
        ILogger<CompositeSessionHistory> logger)
    {
        _redis = redis;
        _mongo = mongo;
        _logger = logger;
    }

    public async Task<List<ChatMessage>> LoadAsync(string sessionId, CancellationToken ct = default)
    {
        var fromRedis = await _redis.LoadAsync(sessionId, ct);
        if (fromRedis.Count > 0)
            return fromRedis;

        _logger.LogDebug("Redis miss — MongoDB'den yükleniyor ({SessionId})", sessionId);

        var fromMongo = await _mongo.LoadAsync(sessionId, ct);
        if (fromMongo.Count > 0)
        {
            // Redis'i ısıt — sonraki isteklerde Redis'ten gelsin
            await _redis.SaveAsync(sessionId, fromMongo, ct);
        }

        return fromMongo;
    }

    public async Task SaveAsync(string sessionId, List<ChatMessage> history, CancellationToken ct = default)
    {
        // Her ikisi de bağımsız: biri hata verse diğeri çalışmaya devam eder
        await Task.WhenAll(
            _redis.SaveAsync(sessionId, history, ct),
            _mongo.SaveAsync(sessionId, history, ct));
    }
}
