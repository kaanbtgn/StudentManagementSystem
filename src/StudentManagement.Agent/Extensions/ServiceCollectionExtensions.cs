using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using StudentManagement.Agent.Services;

namespace StudentManagement.Agent.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Azure Document Intelligence ──────────────────────────────────
        services.AddSingleton(_ => new DocumentIntelligenceClient(
            new Uri(configuration["AzureDocumentIntelligence:Endpoint"]
                ?? throw new InvalidOperationException("AzureDocumentIntelligence:Endpoint eksik.")),
            new AzureKeyCredential(configuration["AzureDocumentIntelligence:ApiKey"]
                ?? throw new InvalidOperationException("AzureDocumentIntelligence:ApiKey eksik."))));

        services.AddScoped<AzureDocumentIntelligenceService>();

        // ── McpClient — HTTP transport üzerinden MCP Server'a bağlanır ─
        services.AddSingleton<McpClient>(_ =>
        {
            var mcpUrl = configuration["Mcp:ServerUrl"]
                ?? throw new InvalidOperationException("Mcp:ServerUrl eksik.");

            var transport = new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri(mcpUrl.TrimEnd('/') + "/mcp"),
            });

            return McpClient.CreateAsync(transport).GetAwaiter().GetResult();
        });

        // ── IChatClient — Azure OpenAI + FunctionInvocation middleware ──
        // AzureOpenAIClient  → Azure.AI.OpenAI paketi
        // .AsChatClient()    → Microsoft.Extensions.AI.OpenAI paketi
        // .UseFunctionInvocation() → Microsoft.Extensions.AI (tool calling döngüsü)
        services.AddSingleton<IChatClient>(_ =>
            new AzureOpenAIClient(
                new Uri(configuration["AzureOpenAI:Endpoint"] 
                    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint eksik.")),
                new AzureKeyCredential(configuration["AzureOpenAI:ApiKey"] 
                    ?? throw new InvalidOperationException("AzureOpenAI:ApiKey eksik.")))
            // 1. Önce Azure'un ChatClient'ını deployment adıyla alıyoruz
            .GetChatClient(configuration["AzureOpenAI:DeploymentName"] 
                ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName eksik."))
            // 2. Sonra Microsoft.Extensions.AI uyumlu IChatClient formatına çeviriyoruz
            .AsIChatClient()
            // 3. Builder ile pipeline'ı kuruyoruz
            .AsBuilder()
            .UseFunctionInvocation()
            .Build());
        // ── Orkestratör ─────────────────────────────────────────────────
        services.AddScoped<StudentManagementAgent>();

        return services;
    }
}
