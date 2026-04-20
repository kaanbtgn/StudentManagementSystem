using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StudentManagement.Infrastructure.MongoDB.Documents;

/// <summary>
/// Agent'ın karar sürecini ve akıl yürütme adımlarını tutan log kaydı.
/// </summary>
public sealed class AgentLogDocument
{
    [BsonId]
    public ObjectId Id { get; init; }

    [BsonRepresentation(BsonType.String)]
    public Guid SessionId { get; init; }

    public string AgentId { get; init; } = string.Empty;

    /// <summary>Agent'ın bu adımda ne yapmaya karar verdiğinin açıklaması.</summary>
    public string Reasoning { get; init; } = string.Empty;

    public string? SelectedTool { get; init; }

    public string? InputJson { get; init; }

    public string? OutputJson { get; init; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
