using Microsoft.EntityFrameworkCore;
using StudentManagement.Domain.Entities;
using StudentManagement.Domain.Repositories;
using StudentManagement.Infrastructure.Persistence;

namespace StudentManagement.Infrastructure.Persistence.Repositories;

internal sealed class StudentRepository : IStudentRepository
{
    private readonly StudentDbContext _context;

    public StudentRepository(StudentDbContext context) => _context = context;

    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Students.FindAsync([id], ct);

    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken ct = default)
        => await _context.Students.AsNoTracking().ToListAsync(ct);

    public async Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken ct = default)
        => await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber, ct);

    // ILIKE %term% → pg_trgm GIN indeksi devrede ise işlem verimli çalışır (Adım 8 migration'ında ekleniyor)
    public async Task<IReadOnlyList<Student>> SearchByNameAsync(string searchTerm, CancellationToken ct = default)
        => await _context.Students
            .AsNoTracking()
            .Where(s => EF.Functions.ILike(s.FirstName, $"%{searchTerm}%") ||
                        EF.Functions.ILike(s.LastName, $"%{searchTerm}%"))
            .ToListAsync(ct);

    // pg_trgm similarity() — GIN indeksi ile tek DB round-trip, in-memory Levenshtein gerekmez.
    // similarity(first_name || ' ' || last_name, @query) normalleştirilmiş trigram örtüşmesini kullanır.
    // Threshold düşük tutulur (0.3) çünkü OCR çıktısı harf düşürme/ekleme içerebilir.
    public async Task<IReadOnlyList<(Student Student, double Score)>> FuzzySearchByNameAsync(
        string query, double threshold = 0.3, CancellationToken ct = default)
    {
        // Adım 1: DB'de filtrele + skorla — GIN trigram indeksini kullanır, tüm tablo okunmaz.
        var idScores = await _context.Database
            .SqlQuery<FuzzyRow>($"""
                SELECT "Id", similarity("FirstName" || ' ' || "LastName", {query}) AS "Score"
                FROM students
                WHERE similarity("FirstName" || ' ' || "LastName", {query}) >= {threshold}
                  AND "IsAnonymized" = false
                ORDER BY "Score" DESC
                LIMIT 10
                """)
            .ToListAsync(ct);

        if (idScores.Count == 0)
            return [];

        // Adım 2: Eşleşen ID'ler ile entity yükle — PK üzerinde IN sorgusu, çok hızlı.
        var ids = idScores.Select(r => r.Id).ToList();
        var studentMap = await _context.Students
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, ct);

        // Sırayı (en yüksek skor önce) ve skoru koru; Zip yoktur, dictionary join güvenlidir.
        return idScores
            .Where(r => studentMap.ContainsKey(r.Id))
            .Select(r => (Student: studentMap[r.Id], Score: r.Score))
            .ToList();
    }

    private sealed record FuzzyRow(Guid Id, double Score);


    public async Task<Student> AddAsync(Student student, CancellationToken ct = default)
    {
        await _context.Students.AddAsync(student, ct);
        await _context.SaveChangesAsync(ct);
        return student;
    }

    public async Task UpdateAsync(Student student, CancellationToken ct = default)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync(ct);
    }

    // Kayıt tamamen silinir; anonimleştirme ayrı bir endpoint üzerinden yapılır (KVKK)
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await _context.Students
            .Where(s => s.Id == id)
            .ExecuteDeleteAsync(ct);
}
