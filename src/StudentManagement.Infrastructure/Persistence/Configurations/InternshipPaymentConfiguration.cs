using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Domain.Entities;
using StudentManagement.Domain.Enums;

namespace StudentManagement.Infrastructure.Persistence.Configurations;

internal sealed class InternshipPaymentConfiguration : IEntityTypeConfiguration<InternshipPayment>
{
    public void Configure(EntityTypeBuilder<InternshipPayment> builder)
    {
        builder.ToTable("internship_payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.StudentId)
            .IsRequired();

        builder.HasOne(p => p.Student)
            .WithMany()
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.PeriodYear)
            .IsRequired();

        builder.Property(p => p.PeriodMonth)
            .IsRequired();

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.PaymentDate);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // Bir öğrencinin aynı döneme yalnızca tek ödemesi olabilir
        builder.HasIndex(p => new { p.StudentId, p.PeriodYear, p.PeriodMonth })
            .IsUnique();
    }
}
