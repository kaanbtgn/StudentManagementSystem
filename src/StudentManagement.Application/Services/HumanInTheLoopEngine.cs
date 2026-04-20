using Microsoft.Extensions.Logging;
using StudentManagement.Domain.Entities;
using StudentManagement.Domain.Repositories;

namespace StudentManagement.Application.Services;

// ── Internal result types ─────────────────────────────────────────────────────

internal sealed record ResolvedItem<TRequest>(Guid StudentId, string StudentName, TRequest Request);

internal sealed record AmbiguousItem<TRequest>(
    string OriginalName,
    IReadOnlyList<string> PossibleMatches,
    TRequest Request);

internal sealed record StudentResolutionResult<TRequest>(
    IReadOnlyList<ResolvedItem<TRequest>> Resolved,
    IReadOnlyList<AmbiguousItem<TRequest>> Ambiguous,
    bool NeedsHumanVerification);

// ── Engine ────────────────────────────────────────────────────────────────────

internal sealed class HumanInTheLoopEngine
{
    private const double FuzzyThreshold = 0.80;
    private const double OcrConfidenceThreshold = 0.85;

    private readonly IStudentRepository _studentRepository;
    private readonly Interfaces.IFuzzyMatcher _fuzzyMatcher;
    private readonly ILogger<HumanInTheLoopEngine> _logger;

    public HumanInTheLoopEngine(
        IStudentRepository studentRepository,
        Interfaces.IFuzzyMatcher fuzzyMatcher,
        ILogger<HumanInTheLoopEngine> logger)
    {
        _studentRepository = studentRepository;
        _fuzzyMatcher = fuzzyMatcher;
        _logger = logger;
    }

    /// <summary>
    /// Her request için öğrenci çözümlemesi yapar.
    /// <paramref name="nameSelector"/> ile request'ten öğrenci adı/numarası alınır.
    /// <paramref name="ocrConfidence"/> 0.85 altındaysa tüm batch HiTL'e düşer.
    /// </summary>
    public async Task<StudentResolutionResult<TRequest>> ResolveStudentsAsync<TRequest>(
        IReadOnlyList<TRequest> requests,
        Func<TRequest, string> nameSelector,
        double? ocrConfidence = null,
        CancellationToken ct = default)
    {
        if (ocrConfidence.HasValue && ocrConfidence.Value < OcrConfidenceThreshold)
        {
            _logger.LogWarning(
                "OCR confidence {Score:F2} is below threshold {Threshold}. Entire batch flagged for human verification.",
                ocrConfidence.Value, OcrConfidenceThreshold);

            var allAmbiguous = requests
                .Select(r => new AmbiguousItem<TRequest>(nameSelector(r), [], r))
                .ToList();

            return new StudentResolutionResult<TRequest>([], allAmbiguous, NeedsHumanVerification: true);
        }

        var resolved = new List<ResolvedItem<TRequest>>();
        var ambiguous = new List<AmbiguousItem<TRequest>>();
        bool needsHumanVerification = false;

        foreach (var request in requests)
        {
            var nameOrNumber = nameSelector(request);

            // 1. Önce öğrenci numarası ile tam eşleşme dene
            var byNumber = await _studentRepository.GetByStudentNumberAsync(nameOrNumber, ct);
            if (byNumber is not null)
            {
                var fullName = $"{byNumber.FirstName} {byNumber.LastName}";
                resolved.Add(new ResolvedItem<TRequest>(byNumber.Id, fullName, request));
                continue;
            }

            // 2. İsim araması yap
            var candidates = await _studentRepository.SearchByNameAsync(nameOrNumber, ct);
            if (candidates.Count == 0)
            {
                _logger.LogInformation("No candidate found for '{NameOrNumber}'.", nameOrNumber);
                ambiguous.Add(new AmbiguousItem<TRequest>(nameOrNumber, [], request));
                needsHumanVerification = true;
                continue;
            }

            // 3. Fuzzy scoring
            var candidateNames = candidates
                .Select(s => $"{s.FirstName} {s.LastName}")
                .ToList();

            var matches = _fuzzyMatcher.FindBestMatches(nameOrNumber, candidateNames, FuzzyThreshold);

            if (matches.Count == 1)
            {
                var student = FindStudentByFullName(candidates, matches[0].Value);
                resolved.Add(new ResolvedItem<TRequest>(student.Id, matches[0].Value, request));
                _logger.LogDebug("Resolved '{NameOrNumber}' → '{StudentName}' (score={Score:F2}).",
                    nameOrNumber, matches[0].Value, matches[0].Score);
            }
            else
            {
                // 0 eşleşme (eşik altında kaldı) veya birden fazla eşleşme
                var possibleMatches = matches.Select(m => m.Value).ToList();
                _logger.LogInformation(
                    "Ambiguous match for '{NameOrNumber}': {Count} candidates above threshold.",
                    nameOrNumber, matches.Count);

                ambiguous.Add(new AmbiguousItem<TRequest>(nameOrNumber, possibleMatches, request));
                needsHumanVerification = true;
            }
        }

        return new StudentResolutionResult<TRequest>(resolved, ambiguous, needsHumanVerification);
    }

    private static Student FindStudentByFullName(IReadOnlyList<Student> candidates, string fullName)
        => candidates.First(s => $"{s.FirstName} {s.LastName}" == fullName);
}
