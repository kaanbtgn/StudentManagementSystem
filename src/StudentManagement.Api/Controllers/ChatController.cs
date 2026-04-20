using Microsoft.AspNetCore.Mvc;
using StudentManagement.Application.Interfaces;

namespace StudentManagement.Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly IAgentClient _agent;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IAgentClient agent, ILogger<ChatController> logger)
    {
        _agent = agent;
        _logger = logger;
    }

    /// <summary>
    /// Dosyasız sohbet — düz JSON.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Chat isteği agent'a iletiliyor.");
        string reply = await _agent.ChatAsync(request.Message, ct);
        return Content(reply, "application/json");
    }

    /// <summary>
    /// Dosyalı sohbet — multipart/form-data.
    /// Dosya ve mesaj doğrudan agent'a form-data olarak iletilir; OCR agent'ta yapılır.
    /// </summary>
    [HttpPost("document")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ChatWithDocument(
        [FromForm] string message,
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Dosya boş veya eksik.");

        _logger.LogInformation("Dosyalı chat isteği agent'a iletiliyor. Dosya: {FileName}", file.FileName);

        await using Stream stream = file.OpenReadStream();
        string reply = await _agent.ChatWithDocumentAsync(
            message,
            stream,
            file.FileName,
            file.ContentType,
            ct);

        return Content(reply, "application/json");
    }
}

public sealed record ChatRequest(string Message);
