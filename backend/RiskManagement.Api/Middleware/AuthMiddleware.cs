using RiskManagement.Api.Services;

namespace RiskManagement.Api.Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly HashSet<string> PublicPaths = new()
    {
        "/", "/login", "/auth/callback", "/logout", "/robots.txt", "/favicon.ico", "/api/test/session"
    };

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    private static bool IsPublicPath(string pathname)
    {
        if (PublicPaths.Contains(pathname))
        {
            return true;
        }

        return pathname.StartsWith("/_app/") || pathname.StartsWith("/assets/");
    }

    private static bool IsInquiryEndpoint(string pathname)
    {
        // Inquiry endpoints have mixed role requirements - controller handles role checking
        return pathname.StartsWith("/api/applications") &&
               (pathname.Contains("/inquiry") || pathname.Contains("/answer-inquiry") || pathname.EndsWith("/inquiries"));
    }

    private static bool RequiresApplicant(string pathname)
    {
        if (IsInquiryEndpoint(pathname)) return false;
        return pathname.StartsWith("/applications") || pathname.StartsWith("/api/applications");
    }

    private static bool RequiresProcessor(string pathname)
    {
        return pathname.StartsWith("/processor") || pathname.StartsWith("/api/processor");
    }

    public async Task InvokeAsync(HttpContext context, SessionService sessionService)
    {
        var pathname = context.Request.Path.Value ?? "/";
        var isApiRoute = pathname.StartsWith("/api/");

        var sessionId = context.Request.Cookies[SessionService.SessionCookieName];
        var user = sessionService.GetSession(sessionId);

        if (user != null)
        {
            context.Items["User"] = user;
        }

        if (IsPublicPath(pathname))
        {
            await _next(context);
            return;
        }

        // Allow static files to pass through
        if (pathname.Contains('.') && !isApiRoute)
        {
            await _next(context);
            return;
        }

        if (user == null)
        {
            if (isApiRoute)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Login erforderlich" });
                return;
            }

            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Login erforderlich");
            return;
        }

        if (RequiresApplicant(pathname) && user.Role != "applicant")
        {
            if (isApiRoute)
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Keine Berechtigung" });
                return;
            }

            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Keine Berechtigung");
            return;
        }

        if (RequiresProcessor(pathname) && user.Role != "processor")
        {
            if (isApiRoute)
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Keine Berechtigung" });
                return;
            }

            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Keine Berechtigung");
            return;
        }

        await _next(context);
    }
}
