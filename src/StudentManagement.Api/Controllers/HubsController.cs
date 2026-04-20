using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StudentManagement.Infrastructure.Realtime;

namespace StudentManagement.Api.Controllers;

/// <summary>
/// SignalR grup yönetimi için HTTP endpoint'leri.
/// Hub metodları yerine IHubContext üzerinden çalışır; test edilebilirlik sağlar.
/// </summary>
[ApiController]
[Route("api/hubs")]
public sealed class HubsController : ControllerBase
{
    private readonly IHubContext<AgentHub> _hubContext;

    public HubsController(IHubContext<AgentHub> hubContext)
        => _hubContext = hubContext;

    /// <summary>
    /// İstemciyi belirtilen sessionId grubuna ekler.
    /// İstemci, SignalR bağlantısından aldığı connectionId'yi gönderir.
    /// </summary>
    [HttpPost("sessions/{sessionId}/join")]
    public async Task<IActionResult> JoinSessionAsync(
        string sessionId,
        [FromBody] HubSessionRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ConnectionId))
            return BadRequest("connectionId zorunludur.");

        await _hubContext.Groups.AddToGroupAsync(request.ConnectionId, sessionId, ct);
        return NoContent();
    }

    /// <summary>
    /// İstemciyi belirtilen sessionId grubundan çıkarır.
    /// </summary>
    [HttpPost("sessions/{sessionId}/leave")]
    public async Task<IActionResult> LeaveSessionAsync(
        string sessionId,
        [FromBody] HubSessionRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ConnectionId))
            return BadRequest("connectionId zorunludur.");

        await _hubContext.Groups.RemoveFromGroupAsync(request.ConnectionId, sessionId, ct);
        return NoContent();
    }
}

public sealed record HubSessionRequest(string ConnectionId);
