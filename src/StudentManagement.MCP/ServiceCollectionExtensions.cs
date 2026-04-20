using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StudentManagement.MCP;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMcpHttpClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string apiBaseUrl = configuration["Api:BaseUrl"]
            ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");

        services.AddHttpClient("StudentManagementApi", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}
