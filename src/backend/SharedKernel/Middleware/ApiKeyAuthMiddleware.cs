using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace SharedKernel.Middleware;

public class ApiKeyAuthMiddleware
{
    private const string ApiKeyHeaderName = "X-Api-Key";
    private const string InternalPathPrefix = "/api/internal/";
    private readonly RequestDelegate _next;
    private readonly string _apiKey;

    public ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _apiKey = configuration["SERVICE_API_KEY"]
                  ?? throw new InvalidOperationException("SERVICE_API_KEY is not configured");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api/internal"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey)
            || string.IsNullOrWhiteSpace(extractedApiKey)
            || extractedApiKey != _apiKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
            return;
        }

        await _next(context);
    }
}