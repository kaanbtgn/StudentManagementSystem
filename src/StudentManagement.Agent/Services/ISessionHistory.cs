using Microsoft.Extensions.AI;

namespace StudentManagement.Agent.Services;

public interface ISessionHistory
{
    Task<List<ChatMessage>> LoadAsync(string sessionId, CancellationToken ct = default);
    Task SaveAsync(string sessionId, List<ChatMessage> history, CancellationToken ct = default);
}
