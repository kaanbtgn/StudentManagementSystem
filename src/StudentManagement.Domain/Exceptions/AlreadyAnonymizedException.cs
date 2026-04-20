namespace StudentManagement.Domain.Exceptions;

public sealed class AlreadyAnonymizedException : DomainException
{
    public AlreadyAnonymizedException(Guid studentId)
        : base($"Öğrenci zaten anonimleştirilmiş. Id: {studentId}") { }
}
