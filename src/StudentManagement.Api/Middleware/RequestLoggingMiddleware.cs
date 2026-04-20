using System.Diagnostics;

namespace StudentManagement.Api.Middleware;

/// <summary>
/// Her HTTP isteğini ve yanıtını yapılandırılmış olarak loglar.
/// SessionIdMiddleware'den sonra pipeline'a eklenmelidir (SessionId zaten scope'ta olur).
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogInformation(
            "HTTP {Method} {Path}{Query} başladı.",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString);

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var level = context.Response.StatusCode >= 500
                ? LogLevel.Error
                : context.Response.StatusCode >= 400
                    ? LogLevel.Warning
                    : LogLevel.Information;

            _logger.Log(
                level,
                "HTTP {Method} {Path} → {StatusCode} ({ElapsedMs} ms).",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);
        }
    }
}
