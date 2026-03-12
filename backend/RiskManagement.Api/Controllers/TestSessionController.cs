using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Models;
using RiskManagement.Api.Services;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Route("api/test/session")]
public class TestSessionController : ControllerBase
{
    private readonly SessionService _sessionService;

    public TestSessionController(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    private static bool IsTestMode()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var testFlag = Environment.GetEnvironmentVariable("TEST");
        return env == "Development" || testFlag == "true";
    }

    [HttpPost]
    public IActionResult CreateTestSession([FromBody] TestSessionCreateDto dto)
    {
        if (!IsTestMode())
        {
            return NotFound(new { error = "Not found" });
        }

        if (string.IsNullOrEmpty(dto.Role) || (dto.Role != "applicant" && dto.Role != "processor"))
        {
            return BadRequest(new { error = "Invalid role" });
        }

        if (string.IsNullOrEmpty(dto.Id))
        {
            return BadRequest(new { error = "Invalid id" });
        }

        if (string.IsNullOrEmpty(dto.Name))
        {
            return BadRequest(new { error = "Invalid name" });
        }

        var defaultEmails = new Dictionary<string, string>
        {
            ["applicant"] = "applicant@example.com",
            ["processor"] = "processor@example.com"
        };

        var user = new UserSession
        {
            Id = dto.Id,
            Email = dto.Email ?? (defaultEmails.ContainsKey(dto.Role) ? defaultEmails[dto.Role] : $"{dto.Role}@example.com"),
            Name = dto.Name,
            Role = dto.Role
        };

        var sessionId = _sessionService.CreateSession(HttpContext, user);

        return Ok(new { sessionId });
    }

    [HttpDelete]
    public IActionResult DeleteTestSession()
    {
        if (!IsTestMode())
        {
            return NotFound(new { error = "Not found" });
        }

        var sessionId = Request.Cookies[SessionService.SessionCookieName];
        _sessionService.DeleteSession(HttpContext, sessionId);

        return NoContent();
    }
}
