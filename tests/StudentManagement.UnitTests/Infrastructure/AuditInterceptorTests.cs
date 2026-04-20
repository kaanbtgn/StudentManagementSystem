using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Moq;
using StudentManagement.Domain.Entities;
using StudentManagement.Domain.Models;
using StudentManagement.Infrastructure.Audit;
using StudentManagement.Infrastructure.Persistence;

namespace StudentManagement.UnitTests.Infrastructure;

public sealed class AuditInterceptorTests
{
    private readonly Mock<IMongoCollection<AuditEntry>> _collection = new();

    private AuditInterceptor BuildInterceptor(HttpContext? httpContext = null)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);
        return new AuditInterceptor(_collection.Object, accessor.Object);
    }

    private static StudentDbContext BuildContext(AuditInterceptor interceptor)
        => new(new DbContextOptionsBuilder<StudentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options);

    private static Student MakeStudent(string phone = "05001234567")
        => new(Guid.NewGuid(), "Ali", "Veli", "2024001", "Bilgisayar",
            DateOnly.FromDateTime(DateTime.Today), phone: phone);

    private List<AuditEntry> CaptureInsertedEntries()
    {
        var captured = new List<AuditEntry>();
        _collection
            .Setup(c => c.InsertManyAsync(
                It.IsAny<IEnumerable<AuditEntry>>(),
                It.IsAny<InsertManyOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<AuditEntry>, InsertManyOptions?, CancellationToken>(
                (entries, _, _) => captured.AddRange(entries))
            .Returns(Task.CompletedTask);
        return captured;
    }

    // --- 1. EntityState.Added → Action = "Create", OldValues = null ---
    [Fact]
    public async Task SavingChangesAsync_NewEntity_CreatesAuditEntryWithCreateAction()
    {
        var captured = CaptureInsertedEntries();
        var interceptor = BuildInterceptor(MakeHttpContext());
        await using var ctx = BuildContext(interceptor);

        ctx.Students.Add(MakeStudent());
        await ctx.SaveChangesAsync();

        captured.Should().ContainSingle();
        captured[0].Action.Should().Be("Create");
        captured[0].OldValues.Should().BeNull();
        captured[0].NewValues.Should().NotBeNull();
    }

    // --- 2. EntityState.Modified → OldValues ve NewValues her ikisi de dolu ---
    [Fact]
    public async Task SavingChangesAsync_ModifiedEntity_CapturesOldAndNewValues()
    {
        var interceptor = BuildInterceptor(MakeHttpContext());
        await using var ctx = BuildContext(interceptor);

        // Create
        var student = MakeStudent();
        CaptureInsertedEntries();
        ctx.Students.Add(student);
        await ctx.SaveChangesAsync();

        // Reset, then capture update
        _collection.Reset();
        var updateCaptured = CaptureInsertedEntries();

        student.FirstName = "Mehmet";
        ctx.Students.Update(student);
        await ctx.SaveChangesAsync();

        updateCaptured.Should().ContainSingle();
        updateCaptured[0].Action.Should().Be("Update");
        updateCaptured[0].OldValues.Should().NotBeNull();
        updateCaptured[0].NewValues.Should().NotBeNull();
    }

    // --- 3. Hassas alan (Phone/Email) → audit'te "[MASKED]" olmalı ---
    // Student entity Email barındırmaz; AuditSensitiveFields her iki alanı da maskeler.
    // Phone üzerinden aynı maskeleme davranışı doğrulanır.
    [Fact]
    public async Task SavingChangesAsync_SensitiveField_MasksInAuditEntry()
    {
        var captured = CaptureInsertedEntries();
        var interceptor = BuildInterceptor(MakeHttpContext());
        await using var ctx = BuildContext(interceptor);

        ctx.Students.Add(MakeStudent(phone: "05559998877"));
        await ctx.SaveChangesAsync();

        captured.Should().ContainSingle();
        captured[0].NewValues.Should().Contain("[MASKED]");
        captured[0].NewValues.Should().NotContain("05559998877");
    }

    // --- 4. IHttpContextAccessor.HttpContext = null → Source = "Agent", SessionId = "system" ---
    [Fact]
    public async Task SavingChangesAsync_NoHttpContext_SetsSourceToSystem()
    {
        var captured = CaptureInsertedEntries();
        var interceptor = BuildInterceptor(httpContext: null);
        await using var ctx = BuildContext(interceptor);

        ctx.Students.Add(MakeStudent());
        await ctx.SaveChangesAsync();

        captured.Should().ContainSingle();
        captured[0].Source.Should().Be("Agent");
        captured[0].SessionId.Should().Be("system");
    }

    private static HttpContext MakeHttpContext(string sessionId = "test-session")
    {
        var ctx = new DefaultHttpContext();
        ctx.Items["SessionId"] = sessionId;
        return ctx;
    }
}
