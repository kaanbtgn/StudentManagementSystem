using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using StudentManagement.Domain.Models;
using StudentManagement.Infrastructure.MongoDB.Documents;

namespace StudentManagement.Infrastructure.MongoDB;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    static MongoDbContext()
    {
        RegisterClassMaps();
    }

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Mongo")
            ?? throw new InvalidOperationException("ConnectionStrings:Mongo is not configured.");

        var mongoUrl = MongoUrl.Create(connectionString);
        var client = new MongoClient(mongoUrl);

        // Database adı connection string'de yoksa appsettings'den okunur
        var databaseName = mongoUrl.DatabaseName
            ?? configuration["MongoDB:DatabaseName"]
            ?? "StudentManagement";

        _database = client.GetDatabase(databaseName);
    }

    /// <summary>
    /// Domain modeli MongoDB BSON'a haritalayan class map'leri kaydeder.
    /// Domain katmanını MongoDB bağımlılığından korur; Infrastructure burada yönetir.
    /// </summary>
    private static void RegisterClassMaps()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(AuditEntry)))
            return;

        BsonClassMap.RegisterClassMap<AuditEntry>(map =>
        {
            map.AutoMap();
            // Id → _id olarak saklansın, Guid string gösterimiyle
            map.MapIdProperty(x => x.Id)
               .SetSerializer(new GuidSerializer(BsonType.String));
            // DateTimeOffset — BsonType.Document olarak native serialize edilir (driver v3)
            map.MapProperty(x => x.Timestamp)
               .SetSerializer(new DateTimeOffsetSerializer(BsonType.String));
        });
    }

    public IMongoCollection<ChatSessionDocument> ChatSessions
        => _database.GetCollection<ChatSessionDocument>("ChatSessions");

    public IMongoCollection<ApplicationLogDocument> ApplicationLogs
        => _database.GetCollection<ApplicationLogDocument>("ApplicationLogs");

    public IMongoCollection<AgentLogDocument> AgentLogs
        => _database.GetCollection<AgentLogDocument>("AgentLogs");

    public IMongoCollection<McpExecutionLogDocument> McpExecutionLogs
        => _database.GetCollection<McpExecutionLogDocument>("McpExecutionLogs");

    public IMongoCollection<AuditEntry> AuditLogs
        => _database.GetCollection<AuditEntry>("AuditLogs");

    public async Task EnsureIndexesAsync()
    {
        // ChatSessions: SessionId + UpdatedAt compound index (context lookup ve TTL için)
        var sessionIndexModel = new CreateIndexModel<ChatSessionDocument>(
            Builders<ChatSessionDocument>.IndexKeys
                .Ascending(s => s.SessionId)
                .Descending(s => s.UpdatedAt),
            new CreateIndexOptions { Name = "idx_session_updated", Background = true });

        await ChatSessions.Indexes.CreateOneAsync(sessionIndexModel);

        // AgentLogs: SessionId + Timestamp
        var agentIndexModel = new CreateIndexModel<AgentLogDocument>(
            Builders<AgentLogDocument>.IndexKeys
                .Ascending(a => a.SessionId)
                .Descending(a => a.Timestamp),
            new CreateIndexOptions { Name = "idx_agent_session_timestamp", Background = true });

        await AgentLogs.Indexes.CreateOneAsync(agentIndexModel);

        // McpExecutionLogs: SessionId + Timestamp
        var mcpIndexModel = new CreateIndexModel<McpExecutionLogDocument>(
            Builders<McpExecutionLogDocument>.IndexKeys
                .Ascending(m => m.SessionId)
                .Descending(m => m.Timestamp),
            new CreateIndexOptions { Name = "idx_mcp_session_timestamp", Background = true });

        await McpExecutionLogs.Indexes.CreateOneAsync(mcpIndexModel);
    }
}
