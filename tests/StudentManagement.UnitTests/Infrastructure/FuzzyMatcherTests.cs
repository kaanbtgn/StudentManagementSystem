using FluentAssertions;
using StudentManagement.Infrastructure.Helpers;

namespace StudentManagement.UnitTests.Infrastructure;

public sealed class FuzzyMatcherTests
{
    private readonly FuzzyMatcher _sut = new();

    [Theory]
    [InlineData("Ahmet Yılmaz", "Ahmet Yılmaz", 1.0000)]
    [InlineData("Ahmet Yılmaz", "Ahmet Ylmaz",  0.9167)] // 1 harf eksik (ı)
    [InlineData("Ahmet Yılmaz", "Mehmet Yılmaz", 0.8462)] // farklı isim
    [InlineData("", "anything", 0.0)]
    public void GetSimilarityScore_VariousInputs_ReturnsExpectedScore(
        string source, string target, double expected)
    {
        var score = _sut.GetSimilarityScore(source, target);

        score.Should().BeApproximately(expected, precision: 0.005);
    }

    [Fact]
    public void FindBestMatches_ThresholdFiltering_ReturnsOnlyAboveThreshold()
    {
        var candidates = new[]
        {
            "Ahmet Yılmaz",  // score ≈ 1.0   → ≥ 0.80
            "Ali Veli",       // score very low → < 0.80
            "Ahmet Ylmaz",   // score ≈ 0.917  → ≥ 0.80
        };

        var matches = _sut.FindBestMatches("Ahmet Yılmaz", candidates, threshold: 0.80);

        matches.Should().HaveCount(2);
        matches.Select(m => m.Value).Should().Contain("Ahmet Yılmaz");
        matches.Select(m => m.Value).Should().Contain("Ahmet Ylmaz");
        matches.Select(m => m.Value).Should().NotContain("Ali Veli");
    }

    [Fact]
    public void FindBestMatches_ResultsOrderedByScoreDescending()
    {
        var candidates = new[] { "Ahmet Ylmaz", "Ahmet Yılmaz" };

        var matches = _sut.FindBestMatches("Ahmet Yılmaz", candidates, threshold: 0.80);

        matches.Should().BeInDescendingOrder(m => m.Score);
        matches[0].Value.Should().Be("Ahmet Yılmaz");
    }

    [Fact]
    public void FindBestMatches_NoAboveThreshold_ReturnsEmptyList()
    {
        var matches = _sut.FindBestMatches("Ahmet Yılmaz", ["Zzz Xyz"], threshold: 0.90);

        matches.Should().BeEmpty();
    }
}
