using StudentManagement.Application.Interfaces;

namespace StudentManagement.Infrastructure.Helpers;

/// <summary>
/// Akış: OCR → Agent (ham metni yorumlar, aday string çıkarır) → FuzzyMatcher (DB kayıtlarına karşı eşleştirir).
/// Levenshtein Distance tabanlı; üçüncü taraf kütüphane kullanılmaz.
/// </summary>
internal sealed class FuzzyMatcher : IFuzzyMatcher
{
    public double GetSimilarityScore(string source, string target)
    {
        if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target)) return 1.0;
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return 0.0;

        int distance = LevenshteinDistance(
            source.ToUpperInvariant(),
            target.ToUpperInvariant());

        int maxLength = Math.Max(source.Length, target.Length);
        return 1.0 - (double)distance / maxLength;
    }

    public IReadOnlyList<FuzzyMatch> FindBestMatches(
        string query,
        IEnumerable<string> candidates,
        double threshold = 0.75)
    {
        return candidates
            .Select(c => new FuzzyMatch(c, GetSimilarityScore(query, c)))
            .Where(m => m.Score >= threshold)
            .OrderByDescending(m => m.Score)
            .ToList();
    }

    // ── Levenshtein Distance (Wagner-Fischer, O(n·m)) ───────────────────────

    private static int LevenshteinDistance(string s, string t)
    {
        int m = s.Length;
        int n = t.Length;

        // Tek satır güncelleme — O(n) bellek
        int[] prev = new int[n + 1];
        int[] curr = new int[n + 1];

        for (int j = 0; j <= n; j++) prev[j] = j;

        for (int i = 1; i <= m; i++)
        {
            curr[0] = i;
            for (int j = 1; j <= n; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                curr[j] = Math.Min(
                    Math.Min(curr[j - 1] + 1, prev[j] + 1),
                    prev[j - 1] + cost);
            }
            (prev, curr) = (curr, prev);
        }

        return prev[n];
    }
}
