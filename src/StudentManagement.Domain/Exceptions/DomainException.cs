namespace StudentManagement.Domain.Exceptions;

/// <summary>
/// Tüm domain exception'larının türetildiği taban sınıf.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Belirtilen mesajla yeni bir <see cref="DomainException"/> örneği oluşturur.
    /// </summary>
    protected DomainException(string message) : base(message) { }

    /// <summary>
    /// Belirtilen mesaj ve iç exception ile yeni bir <see cref="DomainException"/> örneği oluşturur.
    /// </summary>
    protected DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
