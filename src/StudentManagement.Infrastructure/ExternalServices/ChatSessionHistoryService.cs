using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using StudentManagement.Application.Interfaces;
using StudentManagement.Infrastructure.MongoDB;
using StudentManagement.Infrastructure.MongoDB.Documents;

namespace StudentManagement.Infrastructure.ExternalServices;

/// <summary>
/// Konuşma geçmişini Redis (hızlı erişim) ve MongoDB (kalıcı, tam geçmiş) üzerinde yönetir.
///
/// Redis  — sliding window: her zaman son <see cref="MaxMessages"/> mesajı tutar.
///          Okuma: hit → döner; miss → MongoDB'den yükler + ısıtır.
///
/// MongoDB — append-only: tüm session boyunca tüm mesajlar birikim yapar.
///           Geçmişe dokunulmaz; her turda yalnızca yeni mesajlar $push ile eklenir.
/// </summary>
internal sealed class ChatSessionHistoryService : ISessionHistoryService
{
    private static readonly TimeSpan SessionTtl = TimeSpan.FromHours(24);
    private const int MaxMessages = 20;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _cache;
    private readonly MongoDbContext _mongo;
    private readonly ILogger<ChatSessionHistoryService> _logger;

    public ChatSessionHistoryService(
        IDistributedCache cache,
        MongoDbContext mongo,
        ILogger<ChatSessionHistoryService> logger)
    {
        _cache = cache;
        _mongo = mongo;
        _logger = logger;
    }

    /// <summary>Son <see cref="MaxMessages"/> mesajı döner (system hariç).</summary>
    public async Task<List<HistoryEntry>> LoadAsync(string sessionId, CancellationToken ct = default)
    {
        var fromCache = await TryLoadFromCacheAsync(sessionId, ct);
        if (fromCache is { Count: > 0 })
            return fromCache;

        _logger.LogDebug("Redis miss — MongoDB'den yükleniyor ({SessionId})", sessionId);

        var fromMongo = await LoadFromMongoAsync(sessionId, ct);
        if (fromMongo.Count > 0)
            await TrySaveToCacheAsync(sessionId, fromMongo, ct);

        return fromMongo;
    }

    /// <summary>
    /// Bu turda eklenen yeni mesajları kalıcı hale getirir.
    /// MongoDB: $push ile append; Redis: mevcut pencereye ekle, MaxMessages'a kırp.
    /// </summary>
    public async Task AppendAsync(
        string sessionId,
        IReadOnlyList<HistoryEntry> newMessages,
        List<HistoryEntry>? currentCache = null,
        CancellationToken ct = default)
    {
        if (newMessages.Count == 0) return;

        await Task.WhenAll(
            UpdateCacheWindowAsync(sessionId, newMessages, currentCache, ct),
            AppendToMongoAsync(sessionId, newMessages, ct));
    }

    /// <summary>Redis'teki session önbelleğini siler. Session silinirken çağrılır.</summary>
    public async Task ClearCacheAsync(string sessionId, CancellationToken ct = default)
    {
        try
        {
            await _cache.RemoveAsync(CacheKeyFor(sessionId), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Redis session cache temizlenemedi ({SessionId}): {Error}", sessionId, ex.Message);
        }
    }

    // ── Redis ────────────────────────────────────────────────────────────

    private async Task UpdateCacheWindowAsync(
        string sessionId,
        IReadOnlyList<HistoryEntry> newMessages,
        List<HistoryEntry>? currentCache,
        CancellationToken ct)
    {
        try
        {
            // currentCache zaten yüklendiyse ikinci Redis okuma yapmaz
            var current = currentCache ?? await TryLoadFromCacheAsync(sessionId, ct) ?? [];
            var updated = current.Concat(newMessages).TakeLast(MaxMessages).ToList();
            await TrySaveToCacheAsync(sessionId, updated, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Redis sliding window güncellenemedi ({SessionId}): {Error}", sessionId, ex.Message);
        }
    }

    private async Task<List<HistoryEntry>?> TryLoadFromCacheAsync(string sessionId, CancellationToken ct)
    {
        try
        {
            var bytes = await _cache.GetAsync(CacheKeyFor(sessionId), ct);
            if (bytes is null) return null;
            return JsonSerializer.Deserialize<List<HistoryEntry>>(bytes, JsonOpts);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Redis'ten geçmiş yüklenemedi ({SessionId}): {Error}", sessionId, ex.Message);
            return null;
        }
    }

    private async Task TrySaveToCacheAsync(string sessionId, List<HistoryEntry> messages, CancellationToken ct)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(messages, JsonOpts);
            await _cache.SetAsync(
                CacheKeyFor(sessionId),
                bytes,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = SessionTtl },
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Geçmiş Redis'e kaydedilemedi ({SessionId}): {Error}", sessionId, ex.Message);
        }
    }

    // ── MongoDB ──────────────────────────────────────────────────────────

    private async Task<List<HistoryEntry>> LoadFromMongoAsync(string sessionId, CancellationToken ct)
    {
        try
        {
            if (!Guid.TryParse(sessionId, out var guid)) return [];

            var session = await _mongo.ChatSessions
                .Find(s => s.SessionId == guid)
                .FirstOrDefaultAsync(ct);

            if (session is null) return [];

            // Cold-path warm-up: son MaxMessages mesajı döndür (system hariç)
            return session.Messages
                .Where(m => !string.Equals(m.Role, "system", StringComparison.OrdinalIgnoreCase))
                .TakeLast(MaxMessages)
                .Select(m => new HistoryEntry(m.Role, m.Content))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("MongoDB'den geçmiş yüklenemedi ({SessionId}): {Error}", sessionId, ex.Message);
            return [];
        }
    }

    /// <summary>Yeni mesajları $push ile session'a ekler. Geçmiş mesajlara dokunulmaz.</summary>
    private async Task AppendToMongoAsync(string sessionId, IReadOnlyList<HistoryEntry> newMessages, CancellationToken ct)
    {
        try
        {
            if (!Guid.TryParse(sessionId, out var guid)) return;

            var docs = newMessages
                .Select(e => new ChatMessageDocument
                {
                    Role = e.Role,
                    Content = e.Content,
                    Timestamp = DateTime.UtcNow,
                })
                .ToList();

            var update = Builders<ChatSessionDocument>.Update
                .PushEach(s => s.Messages, docs)
                .Set(s => s.UpdatedAt, DateTime.UtcNow)
                .SetOnInsert(s => s.SessionId, guid)
                .SetOnInsert(s => s.CreatedAt, DateTime.UtcNow);

            await _mongo.ChatSessions.UpdateOneAsync(
                s => s.SessionId == guid,
                update,
                new UpdateOptions { IsUpsert = true },
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Mesajlar MongoDB'ye eklenemedi ({SessionId}): {Error}", sessionId, ex.Message);
        }
    }

    private static string CacheKeyFor(string sessionId) => $"agent:session:{sessionId}:history";
}

/// <summary>Konuşma geçmişindeki tek mesaj. Agent ile API arasındaki ortak dil.</summary>
internal sealed record HistoryEntry(string Role, string Content);

