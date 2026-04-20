using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using StudentManagement.Infrastructure.Persistence;

namespace StudentManagement.IntegrationTests.Fixtures;

/// <summary>
/// Spins up real PostgreSQL, Redis and MongoDB containers once per test
/// collection, creates the schema, and tears everything down afterwards.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    // ── containers ──────────────────────────────────────────────────────

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("test_studentmanagement")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .Build();

    private readonly MongoDbContainer _mongo = new MongoDbBuilder()
        .Build();

    // ── connection strings (populated after InitializeAsync) ─────────────

    public string PostgresConnectionString => _postgres.GetConnectionString();
    public string RedisConnectionString    => _redis.GetConnectionString();
    public string MongoConnectionString    => _mongo.GetConnectionString();

    // ── IAsyncLifetime ───────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        // Start all three containers in parallel
        await Task.WhenAll(
            _postgres.StartAsync(),
            _redis.StartAsync(),
            _mongo.StartAsync());

        // Apply EF Core migrations so the schema exists before any test runs
        var options = new DbContextOptionsBuilder<StudentDbContext>()
            .UseNpgsql(PostgresConnectionString)
            .Options;

        await using var ctx = new StudentDbContext(options);
        await ctx.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _postgres.DisposeAsync().AsTask(),
            _redis.DisposeAsync().AsTask(),
            _mongo.DisposeAsync().AsTask());
    }
}
