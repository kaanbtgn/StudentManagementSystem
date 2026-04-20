namespace StudentManagement.Application.Interfaces;

public interface IAgentClient
{
    /// <summary>
    /// Dosyasız sohbet — saf JSON istek.
    /// </summary>
    Task<string> ChatAsync(string message, CancellationToken ct = default);

    /// <summary>
    /// Dosyalı sohbet — multipart/form-data olarak agent'a iletilir.
    /// OCR, agent tarafında çalıştırılır.
    /// </summary>
    Task<string> ChatWithDocumentAsync(
        string message,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken ct = default);
}
