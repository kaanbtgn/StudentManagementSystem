using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using StudentManagement.Api.BackgroundServices;
using StudentManagement.Infrastructure.Persistence;
using StudentManagement.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace StudentManagement.IntegrationTests.Fixtures;

/// <summary>
/// Replaces production connection strings with test-container addresses and
/// removes services that require external infrastructure not available in CI
/// (OCR background service, Agent HTTP client).
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly DatabaseFixture _db;

    public CustomWebApplicationFactory(DatabaseFixture db)
    {
        _db = db;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        // Override connection strings with container addresses
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"]  = _db.PostgresConnectionString,
                ["ConnectionStrings:Redis"]     = _db.RedisConnectionString,
                ["ConnectionStrings:Mongo"]     = _db.MongoConnectionString.TrimEnd('/') + "/test_audit?authSource=admin",
                ["ConnectionStrings:MongoLog"]  = _db.MongoConnectionString,
                // Prevent DI from throwing on missing external config
                ["Agent:BaseUrl"]               = "http://localhost:9999",
                ["AzureDocumentIntelligence:Endpoint"] = "https://placeholder.cognitiveservices.azure.com/",
                ["AzureDocumentIntelligence:ApiKey"]   = "placeholder-key",
                // Satisfy CORS fail-closed check — test runner has no browser origin requirement
                ["AllowedOrigin"]               = "http://localhost",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the hosted OCR background service — it has no real agent in tests
            services.RemoveAll<IHostedService>();

            // Replace the DbContext registration so it points to the test container
            services.RemoveAll<DbContextOptions<StudentDbContext>>();

            services.AddDbContext<StudentDbContext>(opt =>
                opt.UseNpgsql(_db.PostgresConnectionString));
        });
    }
}
