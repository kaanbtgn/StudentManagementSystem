using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Domain.Entities;

namespace StudentManagement.Infrastructure.Persistence.Configurations;

internal sealed class ExamGradeConfiguration : IEntityTypeConfiguration<ExamGrade>
{
    public void Configure(EntityTypeBuilder<ExamGrade> builder)
    {
        builder.ToTable("exam_grades");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .ValueGeneratedNever();

        builder.Property(g => g.StudentId)
            .IsRequired();

        builder.HasOne(g => g.Student)
            .WithMany()
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(g => g.CourseName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Exam1Grade)
            .HasColumnType("decimal(5,2)");

        builder.Property(g => g.Exam2Grade)
            .HasColumnType("decimal(5,2)");

        builder.Property(g => g.CreatedAt)
            .IsRequired();

        builder.Property(g => g.UpdatedAt)
            .IsRequired();

        // Bir öğrencinin aynı ders için tek not kaydı olabilir
        builder.HasIndex(g => new { g.StudentId, g.CourseName })
            .IsUnique();
    }
}
