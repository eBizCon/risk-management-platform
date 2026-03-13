using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace CustomerManagement.Api.Extensions;

public class JsonAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
            return;
        }

        if (authorizeResult.Challenged)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Login erforderlich" });
            return;
        }

        if (authorizeResult.Forbidden)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Keine Berechtigung" });
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
