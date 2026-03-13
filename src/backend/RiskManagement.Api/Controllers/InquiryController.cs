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
[Authorize]
public class InquiryController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public InquiryController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet("api/applications/{id:int}/inquiries")]
    [Authorize(Policy = AuthPolicies.ApplicantOrProcessor)]
    public async Task<IActionResult> GetInquiries(int id)
    {
        var role = User.IsProcessor() ? "processor" : "applicant";
        var result = await _dispatcher.QueryAsync(new GetInquiriesQuery(id, User.GetEmail(), role));
        return result.ToActionResult();
    }

    [HttpPost("api/applications/{id:int}/inquiry")]
    [Authorize(Policy = AuthPolicies.Processor)]
    public async Task<IActionResult> CreateInquiry(int id, [FromBody] InquiryCreateDto dto)
    {
        var result = await _dispatcher.SendAsync(new CreateInquiryCommand(id, dto.InquiryText, User.GetEmail()));
        if (!result.IsSuccess)
            return result.ToActionResult();

        return StatusCode(201, result.Value);
    }

    [HttpPost("api/applications/{id:int}/inquiry/response")]
    [Authorize(Policy = AuthPolicies.Applicant)]
    public async Task<IActionResult> RespondToInquiry(int id, [FromBody] InquiryResponseDto dto)
    {
        var result = await _dispatcher.SendAsync(new AnswerInquiryCommand(id, dto.ResponseText, User.GetEmail()));
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