using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Models;
using RiskManagement.Application.Commands;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Queries;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize(Policy = AuthPolicies.Applicant)]
public class ApplicationsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public ApplicationsController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet]
    public async Task<IActionResult> GetApplications([FromQuery] string? status)
    {
        var result = await _dispatcher.QueryAsync(new GetApplicationsByUserQuery(User.GetEmail(), status));
        return result.ToActionResult();
    }

    [HttpGet("dashboard-stats")]
    [Authorize(Policy = AuthPolicies.ApplicantOrProcessor)]
    public async Task<IActionResult> GetDashboardStats()
    {
        var role = User.IsProcessor() ? "processor" : "applicant";
        var result = await _dispatcher.QueryAsync(new GetDashboardStatsQuery(User.GetEmail(), role));
        return result.ToActionResult();
    }

    [HttpGet("customers")]
    [Authorize(Policy = AuthPolicies.ApplicantOrProcessor)]
    public async Task<IActionResult> GetActiveCustomers()
    {
        var result = await _dispatcher.QueryAsync(new GetActiveCustomersQuery());
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateApplication([FromBody] ApplicationCreateDto dto,
        [FromQuery] bool submit = false)
    {
        if (submit)
        {
            var submitResult = await _dispatcher.SendAsync(new CreateAndSubmitApplicationCommand(dto, User.GetEmail()));
            return submitResult.ToAcceptedResult();
        }

        var result = await _dispatcher.SendAsync(new CreateApplicationCommand(dto, User.GetEmail()));
        return result.ToAcceptedResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = AuthPolicies.ApplicantOrProcessor)]
    public async Task<IActionResult> GetApplication(int id)
    {
        var role = User.IsProcessor() ? "processor" : "applicant";
        var result = await _dispatcher.QueryAsync(new GetApplicationQuery(id, User.GetEmail(), role));
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateApplication(int id, [FromBody] ApplicationUpdateDto dto,
        [FromQuery] bool submit = false)
    {
        if (submit)
        {
            var submitResult =
                await _dispatcher.SendAsync(new UpdateAndSubmitApplicationCommand(id, dto, User.GetEmail()));
            return submitResult.ToActionResult();
        }

        var result = await _dispatcher.SendAsync(new UpdateApplicationCommand(id, dto, User.GetEmail()));
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteApplication(int id)
    {
        var result = await _dispatcher.SendAsync(new DeleteApplicationCommand(id, User.GetEmail()));
        if (!result.IsSuccess)
            return result.ToActionResult();

        return Ok(new { success = true });
    }

    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> SubmitApplication(int id)
    {
        var result = await _dispatcher.SendAsync(new SubmitApplicationCommand(id, User.GetEmail()));
        return result.ToActionResult();
    }
}