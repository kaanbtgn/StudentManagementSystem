namespace StudentManagement.Domain.Exceptions;

/// <summary>
/// Aynı öğrenci numarasına sahip ikinci bir öğrenci eklenmeye çalışıldığında fırlatılır.
/// </summary>
public sealed class DuplicateStudentNumberException : DomainException
{
    /// <summary>
    /// Belirtilen öğrenci numarası için yeni bir <see cref="DuplicateStudentNumberException"/> örneği oluşturur.
    /// </summary>
    public DuplicateStudentNumberException(string studentNumber)
        : base($"'{studentNumber}' öğrenci numarası zaten kayıtlı.") { }
}
