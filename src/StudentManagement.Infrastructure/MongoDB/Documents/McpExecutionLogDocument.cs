using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StudentManagement.Infrastructure.MongoDB.Documents;

/// <summary>
/// MCP tool çalıştırma metriklerini ve sonucunu tutan log kaydı.
/// </summary>
public sealed class McpExecutionLogDocument
{
    [BsonId]
    public ObjectId Id { get; init; }

    [BsonRepresentation(BsonType.String)]
    public Guid SessionId { get; init; }

    public string ToolName { get; init; } = string.Empty;

    public string? ParametersJson { get; init; }

    public string? ResultJson { get; init; }

    public bool Success { get; init; }

    public int DurationMs { get; init; }

    public string? ErrorMessage { get; init; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
