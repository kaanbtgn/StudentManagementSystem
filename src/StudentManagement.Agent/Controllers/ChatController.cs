using Microsoft.AspNetCore.Mvc;
using StudentManagement.Agent.Services;
using StudentManagement.Agent.Services.Models;

namespace StudentManagement.Agent.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly StudentManagementAgent _agent;
    private readonly ILogger<ChatController> _logger;

    public ChatController(StudentManagementAgent agent, ILogger<ChatController> logger)
    {
        _agent = agent;
        _logger = logger;
    }

    /// <summary>
    /// Dosyasız sohbet — düz JSON.
    /// Body: { "sessionId": "...", "message": "..." }
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Chat(
        [FromBody] ChatRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Chat isteği. Session: {SessionId}", request.SessionId);

        var agentRequest = new AgentRequest(request.SessionId, request.Message);
        var response = await _agent.ProcessAsync(agentRequest, ct);

        return Ok(response);
    }

    /// <summary>
    /// Dosyalı sohbet — multipart/form-data.
    /// Fields: sessionId (string), message (string), file (IFormFile)
    /// </summary>
    [HttpPost("document")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ChatWithDocument(
        [FromForm] string sessionId,
        [FromForm] string message,
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Dosya boş veya eksik.");

        _logger.LogInformation(
            "Dosyalı chat isteği. Session: {SessionId}, Dosya: {FileName}",
            sessionId, file.FileName);

        var agentRequest = new AgentRequest(sessionId, message, file);
        var response = await _agent.ProcessAsync(agentRequest, ct);

        return Ok(response);
    }
}

public sealed record ChatRequest(string SessionId, string Message);
