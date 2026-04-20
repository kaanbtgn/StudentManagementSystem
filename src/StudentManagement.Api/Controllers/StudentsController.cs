using Microsoft.AspNetCore.Mvc;
using StudentManagement.Application.DTOs;
using StudentManagement.Application.Interfaces;

namespace StudentManagement.Api.Controllers;

[ApiController]
[Route("api/students")]
public sealed class StudentsController : ControllerBase
{
    private readonly IStudentService _students;
    private readonly IFuzzyMatcher _fuzzy;

    public StudentsController(IStudentService students, IFuzzyMatcher fuzzy)
    {
        _students = students;
        _fuzzy = fuzzy;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken ct)
        => Ok(await _students.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken ct)
        => Ok(await _students.GetByIdAsync(id, ct));
    // StudentNotFoundException → GlobalExceptionMiddleware → 404

    [HttpGet("search")]
    public async Task<IActionResult> SearchAsync([FromQuery] string term, CancellationToken ct)
        => Ok(await _students.SearchAsync(term, ct));

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateStudentRequest req, CancellationToken ct)
    {
        var dto = await _students.CreateAsync(req, ct);
        // ASP.NET Core strips the "Async" suffix from action names by default
        return CreatedAtAction("GetById", new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid id, [FromBody] UpdateStudentRequest req, CancellationToken ct)
    {
        await _students.UpdateAsync(id, req, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        await _students.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// OCR/Agent çıktısından gelen ismi DB adaylarıyla bulanık eşleştirir.
    /// MCP tool, requiresConfirmation=true döndüğünde kullanıcıya "X kişisi mi?" diye sorar,
    /// onay alınmadan mutasyon yapan araçları çağırmaz.
    /// </summary>
    [HttpPost("fuzzy-match")]
    public IActionResult FuzzyMatchAsync([FromBody] FuzzyMatchRequest req)
    {
        var matches = _fuzzy.FindBestMatches(req.Query, req.Candidates, req.Threshold);
        bool requiresConfirmation = !matches.Any() || matches[0].Score < 0.90;

        return Ok(new FuzzyMatchResponse(
            matches.Select(m => new FuzzyMatchItem(m.Value, m.Score)).ToList(),
            requiresConfirmation));
    }
}

public sealed record FuzzyMatchRequest(
    string Query,
    IEnumerable<string> Candidates,
    double Threshold = 0.75);

public sealed record FuzzyMatchItem(string Value, double Score);

public sealed record FuzzyMatchResponse(
    IReadOnlyList<FuzzyMatchItem> Matches,
    bool RequiresConfirmation);
