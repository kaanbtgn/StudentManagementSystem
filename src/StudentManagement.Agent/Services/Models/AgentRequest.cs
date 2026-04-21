namespace StudentManagement.Agent.Services.Models;

/// <summary>
/// Agent'a gönderilen istek. Persistence yok — history API katmanından gelir.
/// </summary>
public sealed record AgentRequest(
    string Message,
    List<ChatHistoryEntry> History,
    IFormFile? File = null
);

/// <summary>Bir konuşma turundaki tek mesajı temsil eden basit DTO.</summary>
public sealed record ChatHistoryEntry(string Role, string Content);
