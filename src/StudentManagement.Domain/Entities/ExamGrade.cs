namespace StudentManagement.Domain.Entities;

/// <summary>
/// Bir öğrenciye ait ders notlarını temsil eder (Sınav 1 ve Sınav 2).
/// </summary>
public sealed class ExamGrade
{
    /// <summary>Kaydın benzersiz tanımlayıcısı.</summary>
    public Guid Id { get; private set; }

    /// <summary>Kaydın ilişkili olduğu öğrencinin kimliği.</summary>
    public Guid StudentId { get; private set; }

    /// <summary>İlişkili öğrenci navigasyon özelliği.</summary>
    public Student Student { get; private set; } = null!;

    /// <summary>Dersin adı.</summary>
    public string CourseName { get; set; }

    /// <summary>Birinci sınav notu. Sınav henüz yapılmamışsa <c>null</c>.</summary>
    public decimal? Exam1Grade { get; set; }

    /// <summary>İkinci sınav notu. Sınav henüz yapılmamışsa <c>null</c>.</summary>
    public decimal? Exam2Grade { get; set; }

    /// <summary>Kaydın oluşturulma zamanı (UTC).</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Kaydın son güncellenme zamanı (UTC).</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Yeni bir <see cref="ExamGrade"/> örneği oluşturur.
    /// </summary>
    /// <param name="id">Benzersiz tanımlayıcı.</param>
    /// <param name="studentId">Öğrenci kimliği.</param>
    /// <param name="courseName">Ders adı.</param>
    /// <param name="exam1Grade">Birinci sınav notu (girilmemişse <c>null</c>).</param>
    /// <param name="exam2Grade">İkinci sınav notu (girilmemişse <c>null</c>).</param>
    /// <param name="createdAt">Oluşturma zamanı.</param>
    /// <param name="updatedAt">Güncelleme zamanı.</param>
    // EF Core design-time ve materialization için gerekli
    private ExamGrade()
    {
        CourseName = null!;
    }

    public ExamGrade(
        Guid id,
        Guid studentId,
        string courseName,
        decimal? exam1Grade,
        decimal? exam2Grade,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        Id = id;
        StudentId = studentId;
        CourseName = courseName;
        Exam1Grade = exam1Grade;
        Exam2Grade = exam2Grade;

        var now = DateTimeOffset.UtcNow;
        CreatedAt = createdAt ?? now;
        UpdatedAt = updatedAt ?? now;
    }
}
