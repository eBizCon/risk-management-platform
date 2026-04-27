using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Models;
using RiskManagement.Application.Queries;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize(Policy = AuthPolicies.ApplicantOrProcessor)]
public class DashboardController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public DashboardController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var userEmail = User.IsProcessor() ? null : User.GetEmail();
        var result = await _dispatcher.QueryAsync(new GetDashboardStatsQuery(userEmail));
        return result.ToActionResult();
    }
}
