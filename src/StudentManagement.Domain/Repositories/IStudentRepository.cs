using StudentManagement.Domain.Entities;

namespace StudentManagement.Domain.Repositories;

/// <summary>
/// Öğrenci verilerine erişim sözleşmesini tanımlar.
/// </summary>
public interface IStudentRepository
{
    /// <summary>Belirtilen kimliğe sahip öğrenciyi döndürür; bulunamazsa <c>null</c>.</summary>
    Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Tüm öğrencilerin listesini döndürür.</summary>
    Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Belirtilen öğrenci numarasına sahip öğrenciyi döndürür; bulunamazsa <c>null</c>.</summary>
    Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken ct = default);

    /// <summary>Ad veya soyad üzerinde arama yaparak eşleşen öğrencileri döndürür.</summary>
    Task<IReadOnlyList<Student>> SearchByNameAsync(string searchTerm, CancellationToken ct = default);

    /// <summary>Yeni öğrenciyi depoya ekler ve eklenen kaydı döndürür.</summary>
    Task<Student> AddAsync(Student student, CancellationToken ct = default);

    /// <summary>Mevcut öğrenci kaydını günceller.</summary>
    Task UpdateAsync(Student student, CancellationToken ct = default);

    /// <summary>Belirtilen kimliğe sahip öğrenciyi siler.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
