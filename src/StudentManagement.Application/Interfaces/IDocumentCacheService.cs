namespace StudentManagement.Application.Interfaces;

public interface IDocumentCacheService
{
    Task<string> StoreDocumentAsync(byte[] content, string fileName, string contentType, CancellationToken ct = default);
    Task<(byte[] Content, string FileName, string ContentType)?> GetDocumentAsync(string fileId, CancellationToken ct = default);
}
