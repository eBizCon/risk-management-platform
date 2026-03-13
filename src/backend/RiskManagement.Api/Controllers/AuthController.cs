using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Data;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Models;

namespace RiskManagement.Api.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationRepository _repository;

    public AuthController(ApplicationRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("login")]
    [HttpPost("login")]
    public IActionResult Login([FromQuery] string? returnTo)
    {
        var redirectUri = "/";
        if (!string.IsNullOrEmpty(returnTo) && returnTo.StartsWith("/"))
        {
            redirectUri = returnTo;
        }

        return Challenge(new AuthenticationProperties { RedirectUri = redirectUri },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        return SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("api/auth/user")]
    public IActionResult GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Ok(new { });
        }

        return Ok(User.ToUserDto());
    }

    [HttpGet("api/dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Ok(new { });
        }

        var role = User.GetRole();
        if (role != "applicant" && role != "processor")
        {
            return Ok(new { });
        }

        var userEmail = role == "applicant" ? User.GetEmail() : null;
        var stats = await _repository.GetDashboardStats(userEmail);
        return Ok(stats);
    }
}
