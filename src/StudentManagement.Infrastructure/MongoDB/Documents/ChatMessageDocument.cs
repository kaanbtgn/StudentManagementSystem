using MongoDB.Bson.Serialization.Attributes;

namespace StudentManagement.Infrastructure.MongoDB.Documents;

/// <summary>
/// Bir sohbet oturumu içindeki tek bir mesajı temsil eder.
/// </summary>
public sealed class ChatMessageDocument
{
    /// <summary>system | user | assistant | tool</summary>
    public string Role { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public List<ToolCallDocument> ToolCalls { get; init; } = [];

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
