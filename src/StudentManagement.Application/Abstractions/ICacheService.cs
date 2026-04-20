namespace StudentManagement.Application.Abstractions;

/// <summary>
/// Genel amaçlı distributed cache sözleşmesi.
/// </summary>
public interface ICacheService
{
    /// <summary>Belirtilen anahtara karşılık gelen değeri döndürür; bulunamazsa <c>null</c>.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>Belirtilen anahtar ile değeri TTL süresiyle depolar.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default);

    /// <summary>Belirtilen anahtarı önbellekten siler.</summary>
    Task RemoveAsync(string key, CancellationToken ct = default);
}
