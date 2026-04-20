namespace StudentManagement.Agent.Services.Models;

public sealed record AgentResponse(
    string Reply,
    bool RequiresConfirmation = false,
    ConfirmationPayload? ConfirmationPayload = null,
    OcrMetadata? OcrMetadata = null
);

public sealed record OcrMetadata(
    double OverallConfidence,
    bool RequiresHumanReview
);
