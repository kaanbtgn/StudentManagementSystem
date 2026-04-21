using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StudentManagement.Api.Models;
using StudentManagement.Application.Interfaces;
using StudentManagement.Infrastructure.MongoDB;
using StudentManagement.Infrastructure.MongoDB.Documents;

namespace StudentManagement.Api.Controllers;

[ApiController]
[Route("api/sessions")]
public sealed class SessionsController : ControllerBase
{
    private static readonly string[] VisibleRoles = ["user", "assistant"];

    private readonly MongoDbContext _mongo;
    private readonly ISessionHistoryService _sessionHistory;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(
        MongoDbContext mongo,
        ISessionHistoryService sessionHistory,
        ILogger<SessionsController> logger)
    {
        _mongo = mongo;
        _sessionHistory = sessionHistory;
        _logger = logger;
    }

    /// <summary>
    /// Yeni boş bir sohbet oturumu oluşturur ve sessionId döner.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateSession(CancellationToken ct)
    {
        var doc = new ChatSessionDocument
        {
            SessionId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _mongo.ChatSessions.InsertOneAsync(doc, cancellationToken: ct);

        _logger.LogInformation("Yeni session oluşturuldu: {SessionId}", doc.SessionId);

        return Ok(new CreateSessionResponse(doc.SessionId, doc.CreatedAt));
    }

    /// <summary>
    /// Son 100 oturumu updatedAt azalan sırада döner.
    /// Title: ilk user mesajının ilk 60 karakteri, yoksa oluşturulma tarihi.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSessions(CancellationToken ct)
    {
        var sessions = await _mongo.ChatSessions
            .Find(FilterDefinition<ChatSessionDocument>.Empty)
            .SortByDescending(s => s.UpdatedAt)
            .Limit(100)
            .ToListAsync(ct);

        var dtos = sessions.Select(s =>
        {
            var firstUserMessage = s.Messages
                .FirstOrDefault(m => m.Role == "user");

            var title = firstUserMessage is not null
                ? firstUserMessage.Content.Length > 60
                    ? firstUserMessage.Content[..60] + "…"
                    : firstUserMessage.Content
                : $"Yeni Sohbet – {s.CreatedAt:dd MMM yyyy HH:mm}";

            var visibleCount = s.Messages.Count(m => VisibleRoles.Contains(m.Role));

            return new SessionSummaryDto(s.SessionId, title, s.CreatedAt, s.UpdatedAt, visibleCount);
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Belirli bir oturumun user + assistant mesajlarını döner.
    /// </summary>
    [HttpGet("{sessionId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid sessionId, CancellationToken ct)
    {
        var session = await _mongo.ChatSessions
            .Find(s => s.SessionId == sessionId)
            .FirstOrDefaultAsync(ct);

        if (session is null)
            return NotFound();

        var messages = session.Messages
            .Where(m => VisibleRoles.Contains(m.Role))
            .Select(m => new SessionMessageDto(m.Role, m.Content, m.Timestamp));

        return Ok(messages);
    }

    /// <summary>
    /// Oturumu MongoDB'den siler ve Redis cache'ini temizler.
    /// </summary>
    [HttpDelete("{sessionId:guid}")]
    public async Task<IActionResult> DeleteSession(Guid sessionId, CancellationToken ct)
    {
        var result = await _mongo.ChatSessions
            .DeleteOneAsync(s => s.SessionId == sessionId, ct);

        if (result.DeletedCount == 0)
            return NotFound();

        await _sessionHistory.ClearCacheAsync(sessionId.ToString(), ct);

        _logger.LogInformation("Session silindi: {SessionId}", sessionId);

        return NoContent();
    }
}
