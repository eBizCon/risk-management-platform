using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Models;
using RiskManagement.Application.Queries;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = AuthPolicies.ApplicantOrProcessor)]
public class DashboardController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public DashboardController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var role = User.IsProcessor() ? "processor" : "applicant";
        var result = await _dispatcher.QueryAsync(new GetDashboardStatsQuery(User.GetEmail(), role));
        return result.ToActionResult();
    }
}
