namespace StudentManagement.Domain.Exceptions;

/// <summary>
/// Sınav notu geçersiz bir değer taşıdığında fırlatılır
/// (not 0'dan küçük veya 100'den büyük olduğunda).
/// </summary>
public sealed class InvalidGradeException : DomainException
{
    /// <summary>
    /// Belirtilen geçersiz not değeri için yeni bir <see cref="InvalidGradeException"/> örneği oluşturur.
    /// </summary>
    public InvalidGradeException(string fieldName, decimal value)
        : base($"'{fieldName}' alanı için geçersiz not değeri: {value}. Not 0 ile 100 arasında olmalıdır.") { }
}
