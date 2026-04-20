namespace StudentManagement.Application.DTOs;

public record AmbiguousGradeItem(
    string OriginalName,
    IReadOnlyList<string> PossibleMatches,
    UpsertGradeRequest SourceRequest);
