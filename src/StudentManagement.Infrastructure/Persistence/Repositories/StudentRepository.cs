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
