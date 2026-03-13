using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Models;
using RiskManagement.Application.Queries;

namespace RiskManagement.Api.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly OidcOptions _oidcOptions;
    private readonly IDispatcher _dispatcher;

    public AuthController(
        IOptions<OidcOptions> oidcOptions,
        IDispatcher dispatcher)
    {
        _oidcOptions = oidcOptions.Value;
        _dispatcher = dispatcher;
    }

    [HttpGet("login")]
    [HttpPost("login")]
    public IActionResult Login([FromQuery] string? returnTo)
    {
        var frontendBase = _oidcOptions.RedirectUri.TrimEnd('/');
        var redirectUri = $"{frontendBase}/";
        if (!string.IsNullOrEmpty(returnTo) && returnTo.StartsWith("/")) redirectUri = $"{frontendBase}{returnTo}";

        return Challenge(new AuthenticationProperties { RedirectUri = redirectUri },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        var frontendBase = _oidcOptions.PostLogoutRedirectUri.TrimEnd('/');
        return SignOut(
            new AuthenticationProperties { RedirectUri = $"{frontendBase}/" },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("api/auth/user")]
    public IActionResult GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated != true) return Ok(new { });

        return Ok(User.ToUserDto());
    }

    [HttpGet("api/dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        if (User.Identity?.IsAuthenticated != true) return Ok(new { });

        var role = User.GetRole();
        if (!User.IsApplicant() && !User.IsProcessor()) return Ok(new { });

        var result = await _dispatcher.QueryAsync(new GetDashboardStatsQuery(User.GetEmail(), role));
        return result.ToActionResult();
    }
}