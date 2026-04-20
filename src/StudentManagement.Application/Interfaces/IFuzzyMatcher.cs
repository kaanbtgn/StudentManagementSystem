namespace StudentManagement.Application.Interfaces;

public interface IFuzzyMatcher
{
    double GetSimilarityScore(string source, string target);

    IReadOnlyList<FuzzyMatch> FindBestMatches(string query, IEnumerable<string> candidates, double threshold = 0.75);
}

public record FuzzyMatch(string Value, double Score);
