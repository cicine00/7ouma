using System.Net;
using System.Text.Json;

namespace Catalog.Service.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(ctx, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        var (code, message) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message),
            InvalidOperationException   => (HttpStatusCode.BadRequest, ex.Message),
            KeyNotFoundException        => (HttpStatusCode.NotFound, ex.Message),
            _                          => (HttpStatusCode.InternalServerError, "Une erreur interne s'est produite.")
        };

        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = (int)code;

        return ctx.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = (int)code,
            message,
            timestamp = DateTime.UtcNow
        }));
    }
}
