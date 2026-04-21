using Microsoft.Extensions.Logging;
using StudentManagement.Application.DTOs;
using StudentManagement.Application.Interfaces;
using StudentManagement.Domain.Entities;
using StudentManagement.Domain.Exceptions;
using StudentManagement.Domain.Repositories;

namespace StudentManagement.Application.Services;

internal sealed class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IStudentRepository studentRepository, ILogger<StudentService> logger)
    {
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<StudentDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var student = await _studentRepository.GetByIdAsync(id, ct)
            ?? throw new StudentNotFoundException(id);

        return Map(student);
    }

    public async Task<IReadOnlyList<StudentDto>> GetAllAsync(CancellationToken ct = default)
    {
        var students = await _studentRepository.GetAllAsync(ct);
        return students.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<StudentDto>> SearchAsync(string term, CancellationToken ct = default)
    {
        var students = await _studentRepository.SearchByNameAsync(term, ct);
        return students.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<FuzzyStudentMatch>> FuzzySearchAsync(
        string query, double threshold = 0.3, CancellationToken ct = default)
    {
        var results = await _studentRepository.FuzzySearchByNameAsync(query, threshold, ct);
        return results
            .Select(r => new FuzzyStudentMatch(Map(r.Student), r.Score))
            .ToList();
    }

    public async Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken ct = default)
    {
        var existing = await _studentRepository.GetByStudentNumberAsync(request.StudentNumber, ct);
        if (existing is not null)
            throw new DuplicateStudentNumberException(request.StudentNumber);

        var student = new Student(
            id: Guid.NewGuid(),
            firstName: request.FirstName,
            lastName: request.LastName,
            studentNumber: request.StudentNumber,
            department: request.Department,
            enrollmentDate: request.EnrollmentDate,
            phone: request.Phone);

        var created = await _studentRepository.AddAsync(student, ct);
        _logger.LogInformation("Student created: {StudentId} ({StudentNumber}).", created.Id, created.StudentNumber);

        return Map(created);
    }

    public async Task<StudentDto> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken ct = default)
    {
        var student = await _studentRepository.GetByIdAsync(id, ct)
            ?? throw new StudentNotFoundException(id);

        // Null geçilen alanlar mevcut değeri korur
        if (request.FirstName is not null) student.FirstName = request.FirstName;
        if (request.LastName is not null) student.LastName = request.LastName;
        if (request.Department is not null) student.Department = request.Department;
        if (request.Phone is not null) student.Phone = request.Phone;
        student.UpdatedAt = DateTimeOffset.UtcNow;

        await _studentRepository.UpdateAsync(student, ct);
        _logger.LogInformation("Student updated: {StudentId}.", id);

        return Map(student);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var student = await _studentRepository.GetByIdAsync(id, ct)
            ?? throw new StudentNotFoundException(id);

        await _studentRepository.DeleteAsync(student.Id, ct);
        _logger.LogInformation("Student deleted: {StudentId}.", id);
    }

    private static StudentDto Map(Student s) => new(
        s.Id,
        s.FirstName,
        s.LastName,
        s.StudentNumber,
        s.Department,
        s.Phone,
        s.EnrollmentDate);
}
