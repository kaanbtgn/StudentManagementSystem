using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StudentManagement.Application.Interfaces;

namespace StudentManagement.Infrastructure.Cache;

internal sealed class DocumentCacheService : IDocumentCacheService
{
    private static readonly TimeSpan DocumentTtl = TimeSpan.FromMinutes(30);

    private readonly IDistributedCache _cache;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public DocumentCacheService(IDistributedCache cache) => _cache = cache;

    public async Task<string> StoreDocumentAsync(
        byte[] content,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        var fileId = Guid.NewGuid().ToString();

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = DocumentTtl,
        };

        await _cache.SetAsync($"doc:content:{fileId}", content, cacheOptions, ct);

        var metaBytes = JsonSerializer.SerializeToUtf8Bytes(
            new DocumentMetadata(fileName, contentType), SerializerOptions);

        await _cache.SetAsync($"doc:meta:{fileId}", metaBytes, cacheOptions, ct);

        return fileId;
    }

    public async Task<(byte[] Content, string FileName, string ContentType)?> GetDocumentAsync(
        string fileId,
        CancellationToken ct = default)
    {
        var content = await _cache.GetAsync($"doc:content:{fileId}", ct);
        if (content is null)
            return null;

        var metaBytes = await _cache.GetAsync($"doc:meta:{fileId}", ct);
        if (metaBytes is null)
            return null;

        var meta = JsonSerializer.Deserialize<DocumentMetadata>(metaBytes, SerializerOptions)!;
        return (content, meta.FileName, meta.ContentType);
    }

    private record DocumentMetadata(string FileName, string ContentType);
}
