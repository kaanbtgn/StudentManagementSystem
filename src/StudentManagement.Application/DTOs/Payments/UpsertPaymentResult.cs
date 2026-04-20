namespace StudentManagement.Application.DTOs;

public record UpsertPaymentResult(
    int ProcessedCount,
    int SkippedCount,
    bool NeedsHumanVerification,
    IReadOnlyList<AmbiguousMatchItem> AmbiguousItems);
