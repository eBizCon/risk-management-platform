using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Models;
using RiskManagement.Application.Commands;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Queries;
using ProcessorDecisionDto = RiskManagement.Application.DTOs.ProcessorDecisionDto;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Route("api/processor")]
[Authorize(Policy = AuthPolicies.Processor)]
public class ProcessorController : ControllerBase
{
    private readonly ICommandHandler<ProcessDecisionCommand, ProcessDecisionResult> _decisionHandler;
    private readonly IQueryHandler<GetApplicationQuery, ApplicationResponse> _getHandler;
    private readonly IQueryHandler<GetProcessorApplicationsQuery, ProcessorApplicationsResponse> _listHandler;

    public ProcessorController(
        ICommandHandler<ProcessDecisionCommand, ProcessDecisionResult> decisionHandler,
        IQueryHandler<GetApplicationQuery, ApplicationResponse> getHandler,
        IQueryHandler<GetProcessorApplicationsQuery, ProcessorApplicationsResponse> listHandler)
    {
        _decisionHandler = decisionHandler;
        _getHandler = getHandler;
        _listHandler = listHandler;
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetApplications([FromQuery] string? status, [FromQuery] int? page)
    {
        var safePage = Math.Max(page ?? 1, 1);
        var result = await _listHandler.HandleAsync(new GetProcessorApplicationsQuery(status, safePage));
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetApplication(int id)
    {
        var result = await _getHandler.HandleAsync(new GetApplicationQuery(id, User.GetEmail(), "processor"));
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/decide")]
    public async Task<IActionResult> ProcessDecision(int id, [FromBody] ProcessorDecisionDto dto)
    {
        var result = await _decisionHandler.HandleAsync(new ProcessDecisionCommand(id, dto));
        if (!result.IsSuccess)
            return result.ToActionResult();

        return Ok(new { application = result.Value!.Application, redirect = result.Value.Redirect });
    }
}
