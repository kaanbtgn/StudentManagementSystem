using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StudentManagement.Application.DTOs;
using StudentManagement.Application.Interfaces;
using StudentManagement.Application.Services;
using StudentManagement.Domain.Entities;
using StudentManagement.Domain.Repositories;

namespace StudentManagement.UnitTests.Application;

public sealed class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepo = new();
    private readonly Mock<IStudentRepository> _studentRepo = new();
    private readonly Mock<IFuzzyMatcher> _fuzzyMatcher = new();

    // HumanInTheLoopEngine is a sealed concrete class — build it with mocked deps.
    private HumanInTheLoopEngine BuildEngine() =>
        new(_studentRepo.Object,
            _fuzzyMatcher.Object,
            NullLogger<HumanInTheLoopEngine>.Instance);

    private PaymentService BuildSut() =>
        new(_paymentRepo.Object,
            _studentRepo.Object,
            BuildEngine(),
            NullLogger<PaymentService>.Instance);

    // ── Scenario 1 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpsertStudentPaymentsAsync_ValidRequests_CallsRepositoryUpsert()
    {
        // Arrange
        var student = MakeStudent("Ali", "Veli");
        var request = new UpsertPaymentRequest("Ali Veli", 2025, 1, 5000m, DateOnly.FromDateTime(DateTime.Today));

        _studentRepo.Setup(r => r.GetByStudentNumberAsync("Ali Veli", default))
                    .ReturnsAsync((Student?)null);
        _studentRepo.Setup(r => r.SearchByNameAsync("Ali Veli", default))
                    .ReturnsAsync([student]);
        _fuzzyMatcher
            .Setup(f => f.FindBestMatches("Ali Veli", It.IsAny<IEnumerable<string>>(), It.IsAny<double>()))
            .Returns([new FuzzyMatch("Ali Veli", 0.95)]);

        // Act
        var result = await BuildSut().UpsertStudentPaymentsAsync([request]);

        // Assert
        result.ProcessedCount.Should().Be(1);
        result.NeedsHumanVerification.Should().BeFalse();
        _paymentRepo.Verify(r => r.UpsertAsync(It.IsAny<InternshipPayment>(), default), Times.Once);
    }

    // ── Scenario 2 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpsertStudentPaymentsAsync_AmbiguousStudentName_ReturnsHumanVerificationResult()
    {
        // Arrange
        var s1 = MakeStudent("Ali", "Veli");
        var s2 = MakeStudent("Ali", "Yılmaz");
        var request = new UpsertPaymentRequest("ali", 2025, 2, 5000m, null);

        _studentRepo.Setup(r => r.GetByStudentNumberAsync("ali", default))
                    .ReturnsAsync((Student?)null);
        _studentRepo.Setup(r => r.SearchByNameAsync("ali", default))
                    .ReturnsAsync([s1, s2]);
        _fuzzyMatcher
            .Setup(f => f.FindBestMatches("ali", It.IsAny<IEnumerable<string>>(), It.IsAny<double>()))
            .Returns([new FuzzyMatch("Ali Veli", 0.85), new FuzzyMatch("Ali Yılmaz", 0.82)]);

        // Act
        var result = await BuildSut().UpsertStudentPaymentsAsync([request]);

        // Assert
        result.NeedsHumanVerification.Should().BeTrue();
        result.ProcessedCount.Should().Be(0);
        result.AmbiguousItems.Should().ContainSingle();
        _paymentRepo.Verify(r => r.UpsertAsync(It.IsAny<InternshipPayment>(), default), Times.Never);
    }

    // ── Scenario 3 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpsertStudentPaymentsAsync_EmptyList_ReturnsZeroProcessed()
    {
        // Act
        var result = await BuildSut().UpsertStudentPaymentsAsync([]);

        // Assert
        result.ProcessedCount.Should().Be(0);
        result.NeedsHumanVerification.Should().BeFalse();
        _paymentRepo.Verify(r => r.UpsertAsync(It.IsAny<InternshipPayment>(), default), Times.Never);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Student MakeStudent(string firstName, string lastName)
    {
        var id = Guid.NewGuid();
        return new Student(id, firstName, lastName,
            studentNumber: $"STU{id.ToString()[..4].ToUpper()}",
            department: "Test",
            enrollmentDate: DateOnly.FromDateTime(DateTime.Today));
    }
}
