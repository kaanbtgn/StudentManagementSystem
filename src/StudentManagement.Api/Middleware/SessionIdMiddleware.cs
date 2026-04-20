using NLog;

namespace StudentManagement.Api.Middleware;

public sealed class SessionIdMiddleware
{
    private const string HeaderName = "X-Session-Id";
    private readonly RequestDelegate _next;

    public SessionIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string sessionId = context.Request.Headers.TryGetValue(HeaderName, out var existing)
            && !string.IsNullOrWhiteSpace(existing)
                ? existing.ToString()
                : Guid.NewGuid().ToString("N");

        // NLog ScopeContext — tüm log satırlarına SessionId eklenir (NLog 5+)
        using (ScopeContext.PushProperty("SessionId", sessionId))
        {
            // Request pipeline boyunca erişilebilir
            context.Items["SessionId"] = sessionId;

            // Client kendi session ID'sini takip edebilsin
            context.Response.Headers[HeaderName] = sessionId;

            await _next(context);
        }
    }
}

public static class SessionIdMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionId(this IApplicationBuilder app) =>
        app.UseMiddleware<SessionIdMiddleware>();
}
