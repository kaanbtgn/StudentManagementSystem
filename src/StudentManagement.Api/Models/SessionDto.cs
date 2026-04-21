namespace StudentManagement.Api.Models;

public sealed record SessionSummaryDto(
    Guid SessionId,
    string Title,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int MessageCount
);

public sealed record SessionMessageDto(
    string Role,
    string Content,
    DateTime Timestamp
);

public sealed record CreateSessionResponse(
    Guid SessionId,
    DateTime CreatedAt
);
