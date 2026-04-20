using System.Text.Json;
using StackExchange.Redis;
using StudentManagement.Application.Abstractions;

namespace StudentManagement.Infrastructure.Cache;

internal sealed class RedisCacheService : ICacheService
{
    // Chat context için varsayılan TTL: 20 dakika
    public static readonly TimeSpan DefaultChatTtl = TimeSpan.FromMinutes(20);

    private readonly IDatabase _db;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public RedisCacheService(IConnectionMultiplexer multiplexer)
        => _db = multiplexer.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value.ToString(), SerializerOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        var serialized = JsonSerializer.Serialize(value, SerializerOptions);
        await _db.StringSetAsync(key, serialized, ttl);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
        => await _db.KeyDeleteAsync(key);
}
