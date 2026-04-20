namespace StudentManagement.Domain.Repositories;

/// <summary>
/// Birden fazla repository üzerinde gerçekleştirilen işlemleri
/// tek bir veritabanı işlemi (transaction) olarak işleme sözleşmesini tanımlar.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Öğrenci repository'si.</summary>
    IStudentRepository Students { get; }

    /// <summary>Staj burs ödeme repository'si.</summary>
    IPaymentRepository Payments { get; }

    /// <summary>Sınav notu repository'si.</summary>
    IExamGradeRepository ExamGrades { get; }

    /// <summary>
    /// Beklemedeki tüm değişiklikleri veritabanına atomik olarak yazar.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
