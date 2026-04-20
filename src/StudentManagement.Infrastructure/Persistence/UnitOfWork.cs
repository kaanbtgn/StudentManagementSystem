using StudentManagement.Domain.Repositories;
using StudentManagement.Infrastructure.Persistence;

namespace StudentManagement.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly StudentDbContext _context;

    public IStudentRepository Students { get; }
    public IPaymentRepository Payments { get; }
    public IExamGradeRepository ExamGrades { get; }

    public UnitOfWork(
        StudentDbContext context,
        IStudentRepository students,
        IPaymentRepository payments,
        IExamGradeRepository examGrades)
    {
        _context = context;
        Students = students;
        Payments = payments;
        ExamGrades = examGrades;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
