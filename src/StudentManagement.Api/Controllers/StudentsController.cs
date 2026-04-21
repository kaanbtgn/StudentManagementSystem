using Microsoft.AspNetCore.Mvc;
using StudentManagement.Application.DTOs;
using StudentManagement.Application.Interfaces;

namespace StudentManagement.Api.Controllers;

[ApiController]
[Route("api/students")]
public sealed class StudentsController : ControllerBase
{
    private readonly IStudentService _students;

    public StudentsController(IStudentService students)
        => _students = students;

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

    /// <summary>
    /// pg_trgm similarity() ile DB tarafında bulanık ad araması yapar.
    /// candidates listesine gerek yoktur; eşleşme ve skorlama tek SQL sorgusunda yapılır.
    /// </summary>
    [HttpGet("fuzzy-search")]
    public async Task<IActionResult> FuzzySearchAsync(
        [FromQuery] string q,
        [FromQuery] double threshold = 0.3,
        CancellationToken ct = default)
    {
        var results = await _students.FuzzySearchAsync(q, threshold, ct);
        bool requiresConfirmation = results.Count == 0 || results[0].Score < 0.75;

        return Ok(new
        {
            matches = results.Select(r => new { student = r.Student, score = r.Score }),
            requiresConfirmation
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateStudentRequest req, CancellationToken ct)
    {
        var dto = await _students.CreateAsync(req, ct);
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
}
