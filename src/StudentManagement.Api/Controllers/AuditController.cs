using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StudentManagement.Domain.Models;
using StudentManagement.Infrastructure.MongoDB;

namespace StudentManagement.Api.Controllers;

[ApiController]
[Route("api/audit")]
public sealed class AuditController : ControllerBase
{
    private readonly IMongoCollection<AuditEntry> _auditLogs;

    public AuditController(MongoDbContext mongo)
    {
        _auditLogs = mongo.AuditLogs;
    }

    /// <summary>
    /// Belirtilen öğrenciye ait audit geçmişini döner (son 50 kayıt, azalan sıra).
    /// OldValues ve NewValues içindeki hassas alanlar zaten [MASKED] olarak saklanmıştır.
    /// </summary>
    [HttpGet("students/{studentId:guid}")]
    public async Task<IActionResult> GetStudentAuditHistoryAsync(
        Guid studentId, CancellationToken ct)
    {
        var entries = await _auditLogs
            .Find(x => x.EntityId == studentId.ToString() && x.EntityType == "Student")
            .SortByDescending(x => x.Timestamp)
            .Limit(50)
            .ToListAsync(ct);

        return Ok(entries);
    }
}
