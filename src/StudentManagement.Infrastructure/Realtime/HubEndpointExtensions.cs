using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace StudentManagement.Infrastructure.Realtime;

public static class HubEndpointExtensions
{
    /// <summary>
    /// Tüm SignalR Hub'larını tek bir çağrıyla eşler.
    /// Program.cs'de inline MapHub çağrısı yapılmaz.
    /// </summary>
    public static IEndpointRouteBuilder MapHubs(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<AgentHub>("/hubs/agent");
        return endpoints;
    }
}
