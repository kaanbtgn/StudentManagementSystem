namespace StudentManagement.Agent.Services.Models;

public sealed record AgentRequest(
    string SessionId,
    string Message,
    IFormFile? File = null
);
