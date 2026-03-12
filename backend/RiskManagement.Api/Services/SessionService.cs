using System.Collections.Concurrent;
using RiskManagement.Api.Models;

namespace RiskManagement.Api.Services;

public class SessionRecord
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string? UserIdToken { get; set; }
    public long ExpiresAt { get; set; }
}

public class SessionService
{
    public const string SessionCookieName = "session";
    public const int SessionMaxAgeSeconds = 3600;

    private readonly ConcurrentDictionary<string, SessionRecord> _store = new();

    public string CreateSession(HttpContext context, UserSession user)
    {
        var sessionId = Guid.NewGuid().ToString();
        var expiresAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + SessionMaxAgeSeconds * 1000L;

        var record = new SessionRecord
        {
            Id = sessionId,
            UserId = user.Id,
            UserEmail = user.Email,
            UserName = user.Name,
            UserRole = user.Role,
            UserIdToken = user.IdToken,
            ExpiresAt = expiresAt
        };

        _store[sessionId] = record;

        var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        context.Response.Cookies.Append(SessionCookieName, sessionId, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Secure = !isDev,
            MaxAge = TimeSpan.FromSeconds(SessionMaxAgeSeconds)
        });

        return sessionId;
    }

    public UserSession? GetSession(string? sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return null;
        }

        if (!_store.TryGetValue(sessionId, out var record))
        {
            return null;
        }

        if (record.ExpiresAt <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            _store.TryRemove(sessionId, out _);
            return null;
        }

        return new UserSession
        {
            Id = record.UserId,
            Email = record.UserEmail,
            Name = record.UserName,
            Role = record.UserRole,
            IdToken = record.UserIdToken
        };
    }

    public void DeleteSession(HttpContext context, string? sessionId)
    {
        if (!string.IsNullOrEmpty(sessionId))
        {
            _store.TryRemove(sessionId, out _);
        }

        context.Response.Cookies.Delete(SessionCookieName, new CookieOptions { Path = "/" });
    }

    public void ClearSessions()
    {
        _store.Clear();
    }
}
