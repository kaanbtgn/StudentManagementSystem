using Microsoft.Extensions.Logging;
using StudentManagement.Application.Interfaces;
using StudentManagement.Domain.Exceptions;
using StudentManagement.Domain.Repositories;

namespace StudentManagement.Application.Services;

internal sealed class PrivacyService : IPrivacyService
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<PrivacyService> _logger;

    public PrivacyService(
        IStudentRepository studentRepository,
        ILogger<PrivacyService> logger)
    {
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task AnonymizeStudentAsync(Guid studentId, CancellationToken ct = default)
    {
        var student = await _studentRepository.GetByIdAsync(studentId, ct)
            ?? throw new StudentNotFoundException(studentId);

        if (student.IsAnonymized)
            throw new AlreadyAnonymizedException(studentId);

        var mask = $"[SİLİNDİ-{studentId.ToString()[..8]}]";

        student.FirstName = mask;
        student.LastName = mask;
        student.Phone = null;
        student.IsAnonymized = true;
        student.UpdatedAt = DateTimeOffset.UtcNow;

        await _studentRepository.UpdateAsync(student, ct);

        _logger.LogInformation("Öğrenci anonimleştirildi. Id: {StudentId}", studentId);
    }
}
