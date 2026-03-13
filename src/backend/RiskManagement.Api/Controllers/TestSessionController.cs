using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Models;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Route("api/test/session")]
public class TestSessionController : ControllerBase
{
    private static bool IsTestMode()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var testFlag = Environment.GetEnvironmentVariable("TEST");
        return env == "Development" || testFlag == "true";
    }

    [HttpPost]
    public async Task<IActionResult> CreateTestSession([FromBody] TestSessionCreateDto dto)
    {
        if (!IsTestMode()) return NotFound(new { error = "Not found" });

        if (string.IsNullOrEmpty(dto.Role) || !AppRoles.All.Contains(dto.Role))
            return BadRequest(new { error = "Invalid role" });

        if (string.IsNullOrEmpty(dto.Id)) return BadRequest(new { error = "Invalid id" });

        if (string.IsNullOrEmpty(dto.Name)) return BadRequest(new { error = "Invalid name" });

        var defaultEmails = new Dictionary<string, string>
        {
            [AppRoles.Applicant] = "applicant@example.com",
            [AppRoles.Processor] = "processor@example.com"
        };

        var email = dto.Email ??
                    (defaultEmails.ContainsKey(dto.Role) ? defaultEmails[dto.Role] : $"{dto.Role}@example.com");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, dto.Id),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, dto.Name),
            new(ClaimTypes.Role, dto.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
            });

        return Ok(new { sessionId = "cookie-auth" });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteTestSession()
    {
        if (!IsTestMode()) return NotFound(new { error = "Not found" });

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
    }
}