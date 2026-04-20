namespace StudentManagement.Application.DTOs;

public record UpsertGradeResult(
    int ProcessedCount,
    bool NeedsHumanVerification,
    IReadOnlyList<AmbiguousGradeItem> AmbiguousItems);
