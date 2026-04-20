using StudentManagement.Domain.Enums;

namespace StudentManagement.Domain.Entities;

/// <summary>
/// Bir öğrenciye ait staj burs ödemesini temsil eder.
/// </summary>
public sealed class InternshipPayment
{
    /// <summary>Ödemenin benzersiz tanımlayıcısı.</summary>
    public Guid Id { get; private set; }

    /// <summary>Ödemenin ilişkili olduğu öğrencinin kimliği.</summary>
    public Guid StudentId { get; private set; }

    /// <summary>İlişkili öğrenci navigasyon özelliği.</summary>
    public Student Student { get; private set; } = null!;

    /// <summary>Ödeme döneminin yılı.</summary>
    public int PeriodYear { get; private set; }

    /// <summary>Ödeme döneminin ayı (1-12).</summary>
    public int PeriodMonth { get; private set; }

    /// <summary>Ödeme tutarı.</summary>
    public decimal Amount { get; set; }

    /// <summary>Fiili ödeme tarihi (henüz ödenmemişse null).</summary>
    public DateOnly? PaymentDate { get; set; }

    /// <summary>Ödemenin mevcut durumu.</summary>
    public PaymentStatus Status { get; set; }

    /// <summary>Kaydın oluşturulma zamanı (UTC).</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Kaydın son güncellenme zamanı (UTC).</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Yeni bir <see cref="InternshipPayment"/> örneği oluşturur.
    /// </summary>
    /// <param name="id">Benzersiz tanımlayıcı.</param>
    /// <param name="studentId">Öğrenci kimliği.</param>
    /// <param name="periodYear">Dönem yılı.</param>
    /// <param name="periodMonth">Dönem ayı.</param>
    /// <param name="amount">Tutar.</param>
    /// <param name="status">Ödeme durumu.</param>
    /// <param name="paymentDate">Ödeme tarihi (isteğe bağlı).</param>
    /// <param name="createdAt">Oluşturma zamanı.</param>
    /// <param name="updatedAt">Güncelleme zamanı.</param>
    // EF Core design-time ve materialization için gerekli
    private InternshipPayment() { }

    public InternshipPayment(
        Guid id,
        Guid studentId,
        int periodYear,
        int periodMonth,
        decimal amount,
        PaymentStatus status,
        DateOnly? paymentDate = null,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        Id = id;
        StudentId = studentId;
        PeriodYear = periodYear;
        PeriodMonth = periodMonth;
        Amount = amount;
        Status = status;
        PaymentDate = paymentDate;

        var now = DateTimeOffset.UtcNow;
        CreatedAt = createdAt ?? now;
        UpdatedAt = updatedAt ?? now;
    }
}
