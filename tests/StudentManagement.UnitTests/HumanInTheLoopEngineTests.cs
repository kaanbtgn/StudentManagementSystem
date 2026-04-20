using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StudentManagement.Application.Interfaces;
using StudentManagement.Application.Services;
using StudentManagement.Domain.Entities;
using StudentManagement.Domain.Repositories;

namespace StudentManagement.UnitTests;

public sealed class HumanInTheLoopEngineTests
{
    private readonly Mock<IStudentRepository> _studentRepo = new();
    private readonly Mock<IFuzzyMatcher> _fuzzyMatcher = new();
    private readonly HumanInTheLoopEngine _engine;

    public HumanInTheLoopEngineTests()
    {
        _engine = new HumanInTheLoopEngine(
            _studentRepo.Object,
            _fuzzyMatcher.Object,
            NullLogger<HumanInTheLoopEngine>.Instance);
    }

    [Fact]
    public async Task ResolveStudentsAsync_SingleHighScoreMatch_ReturnsNoVerificationNeeded()
    {
        // Arrange
        var student = MakeStudent("Ali", "Veli");
        var request = new TestRequest("ali veli");

        _studentRepo.Setup(r => r.GetByStudentNumberAsync("ali veli", default))
                    .ReturnsAsync((Student?)null);
        _studentRepo.Setup(r => r.SearchByNameAsync("ali veli", default))
                    .ReturnsAsync([student]);
        _fuzzyMatcher
            .Setup(f => f.FindBestMatches("ali veli", It.IsAny<IEnumerable<string>>(), It.IsAny<double>()))
            .Returns([new FuzzyMatch("Ali Veli", 0.95)]);

        // Act
        var result = await _engine.ResolveStudentsAsync([request], r => r.Name);

        // Assert
        result.Resolved.Should().ContainSingle();
        result.Ambiguous.Should().BeEmpty();
        result.NeedsHumanVerification.Should().BeFalse();
        result.Resolved[0].StudentId.Should().Be(student.Id);
    }

    [Fact]
    public async Task ResolveStudentsAsync_MultipleMatches_ReturnsVerificationNeeded()
    {
        // Arrange
        var s1 = MakeStudent("Ali", "Veli");
        var s2 = MakeStudent("Ali", "Yılmaz");
        var request = new TestRequest("ali");

        _studentRepo.Setup(r => r.GetByStudentNumberAsync("ali", default))
                    .ReturnsAsync((Student?)null);
        _studentRepo.Setup(r => r.SearchByNameAsync("ali", default))
                    .ReturnsAsync([s1, s2]);
        _fuzzyMatcher
            .Setup(f => f.FindBestMatches("ali", It.IsAny<IEnumerable<string>>(), It.IsAny<double>()))
            .Returns([new FuzzyMatch("Ali Veli", 0.85), new FuzzyMatch("Ali Yılmaz", 0.82)]);

        // Act
        var result = await _engine.ResolveStudentsAsync([request], r => r.Name);

        // Assert
        result.Resolved.Should().BeEmpty();
        result.Ambiguous.Should().ContainSingle();
        result.NeedsHumanVerification.Should().BeTrue();
        result.Ambiguous[0].PossibleMatches.Should().HaveCount(2);
    }

    [Fact]
    public async Task ResolveStudentsAsync_NoMatch_ReturnsVerificationNeeded()
    {
        // Arrange
        var request = new TestRequest("bilinmeyen öğrenci");

        _studentRepo.Setup(r => r.GetByStudentNumberAsync("bilinmeyen öğrenci", default))
                    .ReturnsAsync((Student?)null);
        _studentRepo.Setup(r => r.SearchByNameAsync("bilinmeyen öğrenci", default))
                    .ReturnsAsync([]);

        // Act
        var result = await _engine.ResolveStudentsAsync([request], r => r.Name);

        // Assert
        result.Resolved.Should().BeEmpty();
        result.Ambiguous.Should().ContainSingle();
        result.NeedsHumanVerification.Should().BeTrue();
        result.Ambiguous[0].PossibleMatches.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolveStudentsAsync_LowOcrConfidence_AllItemsFlaggedForReview()
    {
        // Arrange
        var requests = new[]
        {
            new TestRequest("Ali Veli"),
            new TestRequest("Ayşe Kaya"),
        };

        // Act — OCR confidence 0.80 < 0.85 threshold
        var result = await _engine.ResolveStudentsAsync(requests, r => r.Name, ocrConfidence: 0.80);

        // Assert
        result.Resolved.Should().BeEmpty();
        result.Ambiguous.Should().HaveCount(2);
        result.NeedsHumanVerification.Should().BeTrue();

        // Repository hiç çağrılmamalı
        _studentRepo.Verify(r => r.GetByStudentNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResolveStudentsAsync_EmptyInputList_ReturnsEmptyResult()
    {
        // Act
        var result = await _engine.ResolveStudentsAsync(
            Array.Empty<TestRequest>(),
            r => r.Name);

        // Assert
        result.Resolved.Should().BeEmpty();
        result.Ambiguous.Should().BeEmpty();
        result.NeedsHumanVerification.Should().BeFalse();
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

    private sealed record TestRequest(string Name);
}
