using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StudentManagement.Infrastructure.MongoDB.Documents;

/// <summary>
/// NLog MongoDB target tarafından yazılan uygulama log kaydını temsil eder.
/// Alan isimleri NLog MongoDBTarget'ın varsayılan şemasıyla uyumludur.
/// </summary>
public sealed class ApplicationLogDocument
{
    [BsonId]
    public ObjectId Id { get; init; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; init; }

    /// <summary>Trace | Debug | Info | Warn | Error | Fatal</summary>
    public string Level { get; init; } = string.Empty;

    public string Logger { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string? Exception { get; init; }

    /// <summary>NLog MDLC üzerinden enjekte edilen korelasyon kimliği.</summary>
    public string? SessionId { get; init; }

    public BsonDocument? Properties { get; init; }
}
