using Microsoft.AspNetCore.Mvc;
using StudentManagement.Domain.Exceptions;

namespace StudentManagement.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
        {
            // İstemci bağlantıyı kapattı — loglama gerekmez, sessizce 499 döndür
            context.Response.StatusCode = 499;
        }
        catch (StudentNotFoundException ex)
        {
            _logger.LogWarning(ex, "Öğrenci bulunamadı: {Message}", ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Not Found", ex.Message);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain kuralı ihlali: {Message}", ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Bad Request", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İşlenmeyen exception: {Message}", ex.Message);

            var detail = _env.IsDevelopment() ? ex.ToString() : null;
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "Beklenmeyen bir hata oluştu.",
                detail);
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        string? exceptionDetail = null)
    {
        if (context.Response.HasStarted) return;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (exceptionDetail is not null)
            problem.Extensions["exception"] = exceptionDetail;

        await context.Response.WriteAsJsonAsync(problem);
    }
}
