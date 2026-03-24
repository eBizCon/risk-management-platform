using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SharedKernel.Middleware;

public class InternalAuthMiddleware
{
    private const string ApiKeyHeaderName = "X-Api-Key";
    private const string UserIdHeader = "X-User-Id";
    private const string UserEmailHeader = "X-User-Email";
    private const string UserNameHeader = "X-User-Name";
    private const string UserRoleHeader = "X-User-Role";

    private readonly RequestDelegate _next;
    private readonly string _apiKey;
    private readonly bool _allowHeaderFallback;

    public InternalAuthMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _next = next;
        _apiKey = configuration["SERVICE_API_KEY"]
                  ?? throw new InvalidOperationException("SERVICE_API_KEY is not configured");
        _allowHeaderFallback = environment.IsDevelopment()
                               || string.Equals(
                                   configuration["ASPNETCORE_ENVIRONMENT"],
                                   "Test",
                                   StringComparison.OrdinalIgnoreCase);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        if (!ValidateApiKey(context))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
            return;
        }

        if (context.Request.Path.StartsWithSegments("/api/internal"))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        if (_allowHeaderFallback && TrySetUserFromHeaders(context))
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { error = "Login erforderlich" });
    }

    private static bool TrySetUserFromHeaders(HttpContext context)
    {
        var userId = context.Request.Headers[UserIdHeader].FirstOrDefault();
        var userRole = context.Request.Headers[UserRoleHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(userRole))
            return false;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, userRole)
        };

        var userEmail = context.Request.Headers[UserEmailHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(userEmail))
            claims.Add(new Claim(ClaimTypes.Email, userEmail));

        var userName = context.Request.Headers[UserNameHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(userName))
            claims.Add(new Claim(ClaimTypes.Name, userName));

        var identity = new ClaimsIdentity(claims, "HeaderFallback");
        context.User = new ClaimsPrincipal(identity);
        return true;
    }

    private bool ValidateApiKey(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            return false;

        return !string.IsNullOrWhiteSpace(extractedApiKey) && extractedApiKey == _apiKey;
    }
}