using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Models;
using RiskManagement.Application.Commands;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Queries;
using ApplicationCreateDto = RiskManagement.Application.DTOs.ApplicationCreateDto;
using ApplicationUpdateDto = RiskManagement.Application.DTOs.ApplicationUpdateDto;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize(Policy = AuthPolicies.Applicant)]
public class ApplicationsController : ControllerBase
{
    private readonly ICommandHandler<CreateApplicationCommand, CreateApplicationResult> _createHandler;
    private readonly ICommandHandler<UpdateApplicationCommand, UpdateApplicationResult> _updateHandler;
    private readonly ICommandHandler<DeleteApplicationCommand, bool> _deleteHandler;
    private readonly ICommandHandler<SubmitApplicationCommand, ApplicationResponse> _submitHandler;
    private readonly IQueryHandler<GetApplicationQuery, ApplicationResponse> _getHandler;
    private readonly IQueryHandler<GetApplicationsByUserQuery, ApplicationResponse[]> _listHandler;

    public ApplicationsController(
        ICommandHandler<CreateApplicationCommand, CreateApplicationResult> createHandler,
        ICommandHandler<UpdateApplicationCommand, UpdateApplicationResult> updateHandler,
        ICommandHandler<DeleteApplicationCommand, bool> deleteHandler,
        ICommandHandler<SubmitApplicationCommand, ApplicationResponse> submitHandler,
        IQueryHandler<GetApplicationQuery, ApplicationResponse> getHandler,
        IQueryHandler<GetApplicationsByUserQuery, ApplicationResponse[]> listHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _submitHandler = submitHandler;
        _getHandler = getHandler;
        _listHandler = listHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetApplications([FromQuery] string? status)
    {
        var result = await _listHandler.HandleAsync(new GetApplicationsByUserQuery(User.GetEmail(), status));
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateApplication([FromBody] ApplicationCreateDto dto)
    {
        var result = await _createHandler.HandleAsync(new CreateApplicationCommand(dto, User.GetEmail()));
        if (!result.IsSuccess)
            return result.ToActionResult();

        return Ok(new { application = result.Value!.Application, redirect = result.Value.Redirect });
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = AuthPolicies.ApplicantOrProcessor)]
    public async Task<IActionResult> GetApplication(int id)
    {
        var role = User.IsProcessor() ? "processor" : "applicant";
        var result = await _getHandler.HandleAsync(new GetApplicationQuery(id, User.GetEmail(), role));
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateApplication(int id, [FromBody] ApplicationUpdateDto dto)
    {
        var result = await _updateHandler.HandleAsync(new UpdateApplicationCommand(id, dto, User.GetEmail()));
        if (!result.IsSuccess)
            return result.ToActionResult();

        return Ok(new { application = result.Value!.Application, redirect = result.Value.Redirect });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteApplication(int id)
    {
        var result = await _deleteHandler.HandleAsync(new DeleteApplicationCommand(id, User.GetEmail()));
        if (!result.IsSuccess)
            return result.ToActionResult();

        return Ok(new { success = true });
    }

    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> SubmitApplication(int id)
    {
        var result = await _submitHandler.HandleAsync(new SubmitApplicationCommand(id, User.GetEmail()));
        return result.ToActionResult();
    }
}
