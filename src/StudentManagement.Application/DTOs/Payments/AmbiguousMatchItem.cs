namespace StudentManagement.Application.DTOs;

public record AmbiguousMatchItem(
    string OriginalName,
    IReadOnlyList<string> PossibleMatches,
    UpsertPaymentRequest SourceRequest);
