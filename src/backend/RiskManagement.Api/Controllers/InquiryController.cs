using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Models;
using RiskManagement.Application.Commands;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Queries;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Authorize]
public class InquiryController : ControllerBase
{
    private readonly ICommandHandler<CreateInquiryCommand, object> _createHandler;
    private readonly ICommandHandler<AnswerInquiryCommand, ApplicationResponse> _answerHandler;
    private readonly IQueryHandler<GetInquiriesQuery, List<ApplicationInquiry>> _listHandler;

    public InquiryController(
        ICommandHandler<CreateInquiryCommand, object> createHandler,
        ICommandHandler<AnswerInquiryCommand, ApplicationResponse> answerHandler,
        IQueryHandler<GetInquiriesQuery, List<ApplicationInquiry>> listHandler)
    {
        _createHandler = createHandler;
        _answerHandler = answerHandler;
        _listHandler = listHandler;
    }

    [HttpGet("api/applications/{id:int}/inquiries")]
    [Authorize(Policy = AuthPolicies.ApplicantOrProcessor)]
    public async Task<IActionResult> GetInquiries(int id)
    {
        var role = User.IsProcessor() ? "processor" : "applicant";
        var result = await _listHandler.HandleAsync(new GetInquiriesQuery(id, User.GetEmail(), role));
        return result.ToActionResult();
    }

    [HttpPost("api/applications/{id:int}/inquiry")]
    [Authorize(Policy = AuthPolicies.Processor)]
    public async Task<IActionResult> CreateInquiry(int id, [FromBody] InquiryCreateDto dto)
    {
        var result = await _createHandler.HandleAsync(new CreateInquiryCommand(id, dto.InquiryText, User.GetEmail()));
        if (!result.IsSuccess)
            return result.ToActionResult();

        return StatusCode(201, result.Value);
    }

    [HttpPost("api/applications/{id:int}/inquiry/response")]
    [Authorize(Policy = AuthPolicies.Applicant)]
    public async Task<IActionResult> RespondToInquiry(int id, [FromBody] InquiryResponseDto dto)
    {
        var result = await _answerHandler.HandleAsync(new AnswerInquiryCommand(id, dto.ResponseText, User.GetEmail()));
        return result.ToActionResult();
    }

    [HttpPost("api/applications/{id:int}/answer-inquiry")]
    [Authorize(Policy = AuthPolicies.Applicant)]
    public async Task<IActionResult> AnswerInquiry(int id, [FromBody] InquiryResponseDto dto)
    {
        return await RespondToInquiry(id, dto);
    }

    [HttpPost("api/processor/{id:int}/inquire")]
    [Authorize(Policy = AuthPolicies.Processor)]
    public async Task<IActionResult> ProcessorInquire(int id, [FromBody] InquiryCreateDto dto)
    {
        return await CreateInquiry(id, dto);
    }
}
