namespace StudentManagement.Infrastructure.MongoDB.Documents;

/// <summary>
/// Agent'ın çağırdığı bir MCP/tool çağrısını temsil eder.
/// </summary>
public sealed class ToolCallDocument
{
    public string ToolName { get; init; } = string.Empty;

    /// <summary>Tool'a gönderilen parametreler (JSON string).</summary>
    public string ParametersJson { get; init; } = string.Empty;

    /// <summary>Tool'dan dönen sonuç (JSON string).</summary>
    public string? ResultJson { get; init; }

    public bool Success { get; init; }
}
