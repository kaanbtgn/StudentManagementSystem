using System.Text.Json;
using Microsoft.Extensions.AI;
using StackExchange.Redis;

namespace StudentManagement.Agent.Services;

internal sealed class RedisSessionHistory : ISessionHistory
{
    // 24 saatlik TTL: kullanıcı oturumu yenilense bile geçmiş korunur
    private static readonly TimeSpan SessionTtl = TimeSpan.FromHours(24);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IDatabase _db;
    private readonly ILogger<RedisSessionHistory> _logger;

    public RedisSessionHistory(IConnectionMultiplexer mux, ILogger<RedisSessionHistory> logger)
    {
        _db = mux.GetDatabase();
        _logger = logger;
    }

    public async Task<List<ChatMessage>> LoadAsync(string sessionId, CancellationToken ct = default)
    {
        try
        {
            var json = await _db.StringGetAsync(KeyFor(sessionId));
            if (json.IsNullOrEmpty) return [];

            var records = JsonSerializer.Deserialize<List<SessionRecord>>(json.ToString(), JsonOpts) ?? [];
            return records
                .Select(r => new ChatMessage(new ChatRole(r.Role), r.Content))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Redis'ten session yüklenemedi ({SessionId}): {Error}", sessionId, ex.Message);
            return [];
        }
    }

    public async Task SaveAsync(string sessionId, List<ChatMessage> history, CancellationToken ct = default)
    {
        try
        {
            var records = history
                .Select(m => new SessionRecord(m.Role.Value, m.Text ?? string.Empty))
                .ToList();

            var json = JsonSerializer.Serialize(records, JsonOpts);
            await _db.StringSetAsync(KeyFor(sessionId), json, SessionTtl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Session Redis'e kaydedilemedi ({SessionId}): {Error}", sessionId, ex.Message);
        }
    }

    private static string KeyFor(string sessionId) => $"agent:session:{sessionId}:history";

    // Serileştirme DTO'su — Domain'i MongoDB/Redis bağımlılığından korur
    private record SessionRecord(string Role, string Content);
}
