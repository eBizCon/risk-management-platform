using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Models;
using RiskManagement.Application.Commands;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Queries;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Route("api/scoring-config")]
[Authorize(Policy = AuthPolicies.RiskManager)]
public class ScoringConfigController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public ScoringConfigController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentConfig()
    {
        var result = await _dispatcher.QueryAsync(new GetScoringConfigQuery());
        return result.ToActionResult();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateConfig([FromBody] ScoringConfigUpdateDto dto)
    {
        var result = await _dispatcher.SendAsync(new UpdateScoringConfigCommand(dto, User.GetEmail()));
        return result.ToActionResult();
    }

    [HttpPost("rescore")]
    public async Task<IActionResult> RescoreOpenApplications()
    {
        var result = await _dispatcher.SendAsync(new RescoreOpenApplicationsCommand());
        return result.ToActionResult();
    }
}
