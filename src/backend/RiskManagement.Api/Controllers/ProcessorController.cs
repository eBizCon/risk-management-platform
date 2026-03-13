using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Models;
using RiskManagement.Application.Commands;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Queries;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Route("api/processor")]
[Authorize(Policy = AuthPolicies.Processor)]
public class ProcessorController : ControllerBase
{
    private readonly ICommandHandler<ApproveApplicationCommand, ApproveApplicationResult> _approveHandler;
    private readonly ICommandHandler<RejectApplicationCommand, RejectApplicationResult> _rejectHandler;
    private readonly IQueryHandler<GetApplicationQuery, ApplicationResponse> _getHandler;
    private readonly IQueryHandler<GetProcessorApplicationsQuery, ProcessorApplicationsResponse> _listHandler;

    public ProcessorController(
        ICommandHandler<ApproveApplicationCommand, ApproveApplicationResult> approveHandler,
        ICommandHandler<RejectApplicationCommand, RejectApplicationResult> rejectHandler,
        IQueryHandler<GetApplicationQuery, ApplicationResponse> getHandler,
        IQueryHandler<GetProcessorApplicationsQuery, ProcessorApplicationsResponse> listHandler)
    {
        _approveHandler = approveHandler;
        _rejectHandler = rejectHandler;
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

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> ApproveApplication(int id, [FromBody] ApproveApplicationDto dto)
    {
        var result = await _approveHandler.HandleAsync(new ApproveApplicationCommand(id, dto));
        if (!result.IsSuccess)
            return result.ToActionResult();

        return Ok(new { application = result.Value!.Application, redirect = result.Value.Redirect });
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> RejectApplication(int id, [FromBody] RejectApplicationDto dto)
    {
        var result = await _rejectHandler.HandleAsync(new RejectApplicationCommand(id, dto));
        if (!result.IsSuccess)
            return result.ToActionResult();

        return Ok(new { application = result.Value!.Application, redirect = result.Value.Redirect });
    }
}
