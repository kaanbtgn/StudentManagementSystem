namespace StudentManagement.Api.Models;

/// <summary>
/// Channel üzerinden OcrBackgroundService'e iletilen iş tanımcısı.
/// Domain nesnesi değildir; iş kuralı taşımaz.
/// </summary>
public sealed record OcrJob(
    Guid JobId,
    string SessionId,
    string TempFilePath,
    string OriginalFileName,
    string Message)
{
    public static OcrJob Create(string sessionId, string tempFilePath, string originalFileName, string message)
        => new(Guid.NewGuid(), sessionId, tempFilePath, originalFileName, message);
}
