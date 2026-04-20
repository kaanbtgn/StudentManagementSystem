namespace StudentManagement.Agent.Services.Models;

public sealed record ConfirmationPayload(
    string MatchedStudentId,
    string OriginalName,
    string MatchedName,
    double Score,
    string PendingAction,
    string PendingActionArgs
);
