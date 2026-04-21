namespace StudentManagement.Domain.Entities;

/// <summary>
/// Sisteme kayıtlı bir öğrenciyi temsil eder.
/// </summary>
public sealed class Student
{
    /// <summary>Öğrencinin benzersiz tanımlayıcısı.</summary>
    public Guid Id { get; private set; }

    /// <summary>Öğrencinin adı.</summary>
    public string FirstName { get; set; }

    /// <summary>Öğrencinin soyadı.</summary>
    public string LastName { get; set; }

    /// <summary>Öğrenci numarası (benzersiz).</summary>
    public string StudentNumber { get; private set; }

    /// <summary>Öğrencinin kayıtlı olduğu bölüm.</summary>
    public string Department { get; set; }

    /// <summary>Öğrencinin telefon numarası (isteğe bağlı).</summary>
    public string? Phone { get; set; }

    /// <summary>Öğrencinin kayıt tarihi.</summary>
    public DateOnly EnrollmentDate { get; private set; }

    /// <summary>Öğrenci verilerinin anonimleştirilip anonimleştirilmediğini belirtir (GDPR).</summary>
    public bool IsAnonymized { get; set; }

    /// <summary>Kaydın oluşturulma zamanı (UTC).</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Kaydın son güncellenme zamanı (UTC).</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Öğrenci verisini KVKK kapsamında anonimleştirir.
    /// StudentNumber unique constraint'ini serbest bırakmak için de maskelenir;
    /// böylece aynı numara yeni bir öğrenciye atanabilir.
    /// </summary>
    public void Anonymize()
    {
        var shortId = Id.ToString()[..8];
        var mask    = $"[SİLİNDİ-{shortId}]";

        FirstName      = mask;
        LastName       = mask;
        Phone          = null;
        StudentNumber  = $"ANON-{shortId}";
        IsAnonymized   = true;
        UpdatedAt      = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Yeni bir <see cref="Student"/> örneği oluşturur.
    /// </summary>
    /// <param name="id">Benzersiz tanımlayıcı.</param>
    /// <param name="firstName">Ad.</param>
    /// <param name="lastName">Soyad.</param>
    /// <param name="studentNumber">Öğrenci numarası.</param>
    /// <param name="department">Bölüm.</param>
    /// <param name="enrollmentDate">Kayıt tarihi.</param>
    /// <param name="phone">Telefon (isteğe bağlı).</param>
    /// <param name="createdAt">Oluşturma zamanı.</param>
    /// <param name="updatedAt">Güncelleme zamanı.</param>
    /// <param name="isAnonymized">Anonimleştirildi mi.</param>
    // EF Core design-time ve materialization için gerekli
    private Student()
    {
        FirstName = null!;
        LastName = null!;
        StudentNumber = null!;
        Department = null!;
    }

    public Student(
        Guid id,
        string firstName,
        string lastName,
        string studentNumber,
        string department,
        DateOnly enrollmentDate,
        string? phone = null,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null,
        bool isAnonymized = false)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        StudentNumber = studentNumber;
        Department = department;
        EnrollmentDate = enrollmentDate;
        Phone = phone;
        IsAnonymized = isAnonymized;

        var now = DateTimeOffset.UtcNow;
        CreatedAt = createdAt ?? now;
        UpdatedAt = updatedAt ?? now;
    }
}
