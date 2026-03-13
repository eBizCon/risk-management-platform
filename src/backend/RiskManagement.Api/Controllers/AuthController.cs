using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Data;
using RiskManagement.Api.Models;
using RiskManagement.Api.Services;

namespace RiskManagement.Api.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly SessionService _sessionService;
    private readonly OidcService _oidcService;
    private readonly IConfiguration _configuration;
    private readonly ApplicationRepository _repository;

    private const int TempCookieMaxAgeSeconds = 300;

    public AuthController(SessionService sessionService, OidcService oidcService, IConfiguration configuration, ApplicationRepository repository)
    {
        _sessionService = sessionService;
        _oidcService = oidcService;
        _configuration = configuration;
        _repository = repository;
    }

    private CookieOptions GetTempCookieOptions()
    {
        var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        return new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Secure = !isDev,
            MaxAge = TimeSpan.FromSeconds(TempCookieMaxAgeSeconds)
        };
    }

    private UserSession? GetUser() => HttpContext.Items["User"] as UserSession;

    [HttpGet("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromQuery] string? returnTo)
    {
        var authRequest = await _oidcService.CreateAuthorizationRequestAsync();

        var options = GetTempCookieOptions();
        Response.Cookies.Append("pkce_verifier", authRequest.CodeVerifier, options);
        Response.Cookies.Append("oidc_state", authRequest.State, options);

        if (!string.IsNullOrEmpty(returnTo) && returnTo.StartsWith("/"))
        {
            Response.Cookies.Append("returnTo", returnTo, options);
        }

        return Redirect(authRequest.Url);
    }

    [HttpGet("auth/callback")]
    public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? state)
    {
        var codeVerifier = Request.Cookies["pkce_verifier"];
        var expectedState = Request.Cookies["oidc_state"];

        if (string.IsNullOrEmpty(codeVerifier) || string.IsNullOrEmpty(expectedState))
        {
            DeleteTempCookies();
            return BadRequest(new { error = "Ungültige Login-Anfrage" });
        }

        if (state != expectedState)
        {
            DeleteTempCookies();
            return BadRequest(new { error = "Ungültige Login-Anfrage" });
        }

        if (string.IsNullOrEmpty(code))
        {
            DeleteTempCookies();
            return BadRequest(new { error = "Ungültige Login-Anfrage" });
        }

        try
        {
            var tokenResult = await _oidcService.ExchangeAuthorizationCodeAsync(code, codeVerifier);

            if (tokenResult.Roles.Count == 0)
            {
                DeleteTempCookies();
                return StatusCode(403, "Keine Berechtigung");
            }

            string userId = "unknown";
            string email = "";
            string name = "Unbekannter Nutzer";

            if (tokenResult.Claims.HasValue)
            {
                var claims = tokenResult.Claims.Value;
                if (claims.TryGetProperty("sub", out var sub))
                    userId = sub.GetString() ?? "unknown";
                if (claims.TryGetProperty("email", out var emailClaim))
                    email = emailClaim.GetString() ?? "";
                if (claims.TryGetProperty("name", out var nameClaim))
                    name = nameClaim.GetString() ?? "Unbekannter Nutzer";
            }

            var user = new UserSession
            {
                Id = userId,
                Email = email,
                Name = name,
                Role = tokenResult.Roles[0],
                IdToken = tokenResult.IdToken
            };

            _sessionService.CreateSession(HttpContext, user);

            var returnTo = Request.Cookies["returnTo"];
            DeleteTempCookies();

            if (!string.IsNullOrEmpty(returnTo) && returnTo.StartsWith("/"))
            {
                return Redirect(returnTo);
            }

            return Redirect("/");
        }
        catch (Exception)
        {
            DeleteTempCookies();
            return BadRequest(new { error = "Login fehlgeschlagen" });
        }
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        var sessionId = Request.Cookies[SessionService.SessionCookieName];
        var sessionUser = _sessionService.GetSession(sessionId);

        _sessionService.DeleteSession(HttpContext, sessionId);

        if (sessionUser?.IdToken != null)
        {
            var logoutUrl = await _oidcService.BuildLogoutUrlAsync(sessionUser.IdToken);
            if (!string.IsNullOrEmpty(logoutUrl))
            {
                return Redirect(logoutUrl);
            }
        }

        var postLogoutRedirectUri = Environment.GetEnvironmentVariable("OIDC_POST_LOGOUT_REDIRECT_URI") ?? "/";
        return Redirect(postLogoutRedirectUri);
    }

    [HttpGet("api/auth/user")]
    public IActionResult GetCurrentUser()
    {
        var user = GetUser();
        if (user == null)
        {
            return Ok(new { });
        }

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role
        });
    }

    [HttpGet("api/dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var user = GetUser();
        if (user == null || (user.Role != "applicant" && user.Role != "processor"))
        {
            return Ok(new { });
        }

        var userEmail = user.Role == "applicant" ? user.Email : null;
        var stats = await _repository.GetDashboardStats(userEmail);
        return Ok(stats);
    }

    private void DeleteTempCookies()
    {
        Response.Cookies.Delete("pkce_verifier", new CookieOptions { Path = "/" });
        Response.Cookies.Delete("oidc_state", new CookieOptions { Path = "/" });
        Response.Cookies.Delete("returnTo", new CookieOptions { Path = "/" });
    }
}
