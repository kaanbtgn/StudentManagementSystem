namespace StudentManagement.Application.Interfaces;

public interface IPrivacyService
{
    Task AnonymizeStudentAsync(Guid studentId, CancellationToken ct = default);
}
