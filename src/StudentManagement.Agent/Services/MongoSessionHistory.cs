using Microsoft.Extensions.AI;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace StudentManagement.Agent.Services;

/// <summary>
/// Chat session geçmişini MongoDB ChatSessions koleksiyonuna yazar.
/// Infrastructure.MongoDB.Documents.ChatSessionDocument ile aynı şemayı takip eder.
/// Sadece son <see cref="MaxMessages"/> mesajı yükler (token tasarrufu).
/// </summary>
internal sealed class MongoSessionHistory : ISessionHistory
{
    private const int MaxMessages = 20;
    private const string CollectionName = "ChatSessions";

    private readonly IMongoCollection<AgentChatSession> _collection;
    private readonly ILogger<MongoSessionHistory> _logger;

    public MongoSessionHistory(IMongoDatabase database, ILogger<MongoSessionHistory> logger)
    {
        _collection = database.GetCollection<AgentChatSession>(CollectionName);
        _logger = logger;
    }

    public async Task<List<ChatMessage>> LoadAsync(string sessionId, CancellationToken ct = default)
    {
        try
        {
            var session = await _collection
                .Find(s => s.SessionId == sessionId)
                .FirstOrDefaultAsync(ct);

            if (session is null) return [];

            return session.Messages
                .TakeLast(MaxMessages)
                .Select(m => new ChatMessage(new ChatRole(m.Role), m.Content))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("MongoDB'den session yüklenemedi ({SessionId}): {Error}", sessionId, ex.Message);
            return [];
        }
    }

    public async Task SaveAsync(string sessionId, List<ChatMessage> history, CancellationToken ct = default)
    {
        try
        {
            var messages = history
                .Select(m => new AgentChatMessage
                {
                    Role = m.Role.Value,
                    Content = m.Text ?? string.Empty,
                    Timestamp = DateTime.UtcNow,
                })
                .ToList();

            var update = Builders<AgentChatSession>.Update
                .Set(s => s.Messages, messages)
                .Set(s => s.UpdatedAt, DateTime.UtcNow)
                .SetOnInsert(s => s.SessionId, sessionId)
                .SetOnInsert(s => s.CreatedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(
                s => s.SessionId == sessionId,
                update,
                new UpdateOptions { IsUpsert = true },
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Session MongoDB'ye kaydedilemedi ({SessionId}): {Error}", sessionId, ex.Message);
        }
    }

    // Infrastructure.MongoDB.Documents.ChatSessionDocument ile şema uyumlu
    private sealed class AgentChatSession
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; init; } = Guid.NewGuid();

        public string SessionId { get; init; } = string.Empty;

        public List<AgentChatMessage> Messages { get; set; } = [];

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    private sealed class AgentChatMessage
    {
        public string Role { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}
