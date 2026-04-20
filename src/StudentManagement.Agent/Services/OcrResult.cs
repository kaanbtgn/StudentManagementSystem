namespace StudentManagement.Agent.Services;

public record OcrResult(
    string RawContent,
    double OverallConfidence,
    bool RequiresHumanReview
);
