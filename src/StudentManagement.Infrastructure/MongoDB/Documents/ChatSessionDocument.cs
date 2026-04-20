using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StudentManagement.Infrastructure.MongoDB.Documents;

/// <summary>
/// Bir kullanıcı sohbet oturumunu ve içindeki tüm mesajları temsil eder.
/// </summary>
public sealed class ChatSessionDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; init; } = Guid.NewGuid();

    [BsonRepresentation(BsonType.String)]
    public Guid SessionId { get; init; }

    public string? UserId { get; init; }

    public List<ChatMessageDocument> Messages { get; init; } = [];

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
