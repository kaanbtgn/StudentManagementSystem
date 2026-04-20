namespace StudentManagement.Domain.Exceptions;

/// <summary>
/// İstenilen kimliğe sahip öğrenci bulunamadığında fırlatılır.
/// </summary>
public sealed class StudentNotFoundException : DomainException
{
    /// <summary>
    /// Belirtilen öğrenci kimliği için yeni bir <see cref="StudentNotFoundException"/> örneği oluşturur.
    /// </summary>
    public StudentNotFoundException(Guid studentId)
        : base($"'{studentId}' kimlikli öğrenci bulunamadı.") { }

    /// <summary>
    /// Belirtilen öğrenci numarası için yeni bir <see cref="StudentNotFoundException"/> örneği oluşturur.
    /// </summary>
    public StudentNotFoundException(string studentNumber)
        : base($"'{studentNumber}' numaralı öğrenci bulunamadı.") { }
}
