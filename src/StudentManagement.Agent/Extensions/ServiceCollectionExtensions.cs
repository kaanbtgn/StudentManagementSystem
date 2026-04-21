using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using MongoDB.Driver;
using StackExchange.Redis;
using StudentManagement.Agent.Services;

namespace StudentManagement.Agent.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Redis — hızlı cache (opsiyonel) ──────────────────────────────────
        var redisConnStr = configuration.GetConnectionString("Redis");
        var hasRedis = false;
        if (!string.IsNullOrWhiteSpace(redisConnStr))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConnStr));
            services.AddScoped<RedisSessionHistory>();
            hasRedis = true;
        }

        // ── MongoDB — kalıcı session geçmişi ─────────────────────────────────
        var mongoConnStr = configuration.GetConnectionString("Mongo");
        var hasMongo = false;
        if (!string.IsNullOrWhiteSpace(mongoConnStr))
        {
            var mongoUrl = MongoUrl.Create(mongoConnStr);
            var mongoClient = new MongoClient(mongoUrl);
            var mongoDb = mongoClient.GetDatabase(
                mongoUrl.DatabaseName
                ?? configuration["MongoDB:DatabaseName"]
                ?? "StudentManagement");

            services.AddSingleton<IMongoDatabase>(_ => mongoDb);
            services.AddScoped<MongoSessionHistory>();
            hasMongo = true;
        }

        // Composite: her ikisi varsa → dual-write; yalnızca biri varsa → tek provider
        if (hasRedis && hasMongo)
            services.AddScoped<ISessionHistory, CompositeSessionHistory>();
        else if (hasRedis)
            services.AddScoped<ISessionHistory>(sp => sp.GetRequiredService<RedisSessionHistory>());
        else if (hasMongo)
            services.AddScoped<ISessionHistory>(sp => sp.GetRequiredService<MongoSessionHistory>());

        // ── Azure Document Intelligence (opsiyonel — yalnızca dosya yüklendiğinde kullanılır) ──
        var docIntelEndpoint = configuration["AzureDocumentIntelligence:Endpoint"];
        var docIntelKey = configuration["AzureDocumentIntelligence:ApiKey"];

        if (!string.IsNullOrWhiteSpace(docIntelEndpoint)
            && !string.IsNullOrWhiteSpace(docIntelKey)
            && Uri.TryCreate(docIntelEndpoint, UriKind.Absolute, out var docIntelUri)
            && !docIntelUri.Host.Contains('<'))  // placeholder guard
        {
            services.AddSingleton(_ => new DocumentIntelligenceClient(
                docIntelUri,
                new AzureKeyCredential(docIntelKey)));

            services.AddScoped<AzureDocumentIntelligenceService>();
        }

        // ── McpClient — HTTP transport üzerinden MCP Server'a bağlanır ─
        // Lazy<T> ile sarılıyor: DI registration anında değil, ilk kullanımda bağlantı kurulur
        services.AddSingleton<Lazy<Task<McpClient>>>(_ =>
        {
            var mcpUrl = configuration["Mcp:ServerUrl"]
                ?? throw new InvalidOperationException("Mcp:ServerUrl eksik.");

            return new Lazy<Task<McpClient>>(() =>
            {
                var transport = new HttpClientTransport(new HttpClientTransportOptions
                {
                    Endpoint = new Uri(mcpUrl.TrimEnd('/') + "/mcp"),
                });
                return McpClient.CreateAsync(transport);
            });
        });

        // ── IChatClient — Azure OpenAI + FunctionInvocation middleware ──
        var openAiEndpoint = configuration["AzureOpenAI:Endpoint"];
        var openAiKey = configuration["AzureOpenAI:ApiKey"];
        var deploymentName = configuration["AzureOpenAI:DeploymentName"];

        if (!string.IsNullOrWhiteSpace(openAiEndpoint) && !string.IsNullOrWhiteSpace(openAiKey) && !string.IsNullOrWhiteSpace(deploymentName))
        {
            services.AddSingleton<IChatClient>(_ =>
                new AzureOpenAIClient(
                    new Uri(openAiEndpoint),
                    new AzureKeyCredential(openAiKey))
                .GetChatClient(deploymentName)
                .AsIChatClient()
                .AsBuilder()
                .UseFunctionInvocation()
                .Build());
        }
        // ── Orkestratör ─────────────────────────────────────────────────
        services.AddScoped<StudentManagementAgent>();

        return services;
    }
}
