namespace StudentManagement.Application.Interfaces;

/// <summary>
/// Session konuşma geçmişi üzerindeki operasyonlar.
/// Infrastructure'daki ChatSessionHistoryService bu interface'i implement eder.
/// </summary>
public interface ISessionHistoryService
{
    /// <summary>Redis'teki session önbelleğini siler. Session silinirken çağrılır.</summary>
    Task ClearCacheAsync(string sessionId, CancellationToken ct = default);
}
