using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Models;
using RiskManagement.Application.Commands;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Queries;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Route("api/processor")]
[Authorize(Policy = AuthPolicies.Processor)]
public class ProcessorController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public ProcessorController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetApplications([FromQuery] string? status, [FromQuery] int? page)
    {
        var safePage = Math.Max(page ?? 1, 1);
        var result = await _dispatcher.QueryAsync(new GetProcessorApplicationsQuery(status, safePage));
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetApplication(int id)
    {
        var result = await _dispatcher.QueryAsync(new GetApplicationQuery(id, User.GetEmail(), "processor"));
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> ApproveApplication(int id, [FromBody] ApproveApplicationDto dto)
    {
        var result = await _dispatcher.SendAsync(new ApproveApplicationCommand(id, dto));
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> RejectApplication(int id, [FromBody] RejectApplicationDto dto)
    {
        var result = await _dispatcher.SendAsync(new RejectApplicationCommand(id, dto));
        return result.ToActionResult();
    }
}