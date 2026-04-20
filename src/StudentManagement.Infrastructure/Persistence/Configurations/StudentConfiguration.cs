using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Domain.Entities;

namespace StudentManagement.Infrastructure.Persistence.Configurations;

internal sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.StudentNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(s => s.StudentNumber)
            .IsUnique();

        // Ad/soyad üzerinde ILIKE araması için pg_trgm GIN index
        // → migration her üretildiğinde otomatik dahil edilir, elle düzenleme gerekmez
        builder.HasIndex(s => new { s.FirstName, s.LastName })
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops", "gin_trgm_ops")
            .HasDatabaseName("idx_students_name_trgm");

        builder.Property(s => s.Department)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Phone)
            .HasMaxLength(20);

        builder.Property(s => s.EnrollmentDate)
            .IsRequired();

        builder.Property(s => s.IsAnonymized)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired();
    }
}
