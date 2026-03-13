using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Data;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Models;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Authorize]
public class InquiryController : ControllerBase
{
    private readonly ApplicationRepository _repository;

    public InquiryController(ApplicationRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("api/applications/{id:int}/inquiries")]
    [Authorize(Policy = AuthPolicies.ApplicantOrProcessor)]
    public async Task<IActionResult> GetInquiries(int id)
    {
        var application = await _repository.GetApplicationById(id);
        if (application == null)
        {
            return NotFound(new { error = "Antrag nicht gefunden" });
        }

        if (User.IsApplicant() && application.CreatedBy != User.GetEmail())
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        var inquiries = await _repository.GetApplicationInquiries(id);
        return Ok(inquiries);
    }

    [HttpPost("api/applications/{id:int}/inquiry")]
    [Authorize(Policy = AuthPolicies.Processor)]
    public async Task<IActionResult> CreateInquiry(int id, [FromBody] InquiryCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.InquiryText))
        {
            return BadRequest(new { errors = new { inquiryText = new[] { "Rückfragetext darf nicht leer sein" } } });
        }

        var application = await _repository.GetApplicationById(id);
        if (application == null)
        {
            return NotFound(new { error = "Antrag nicht gefunden" });
        }

        if (application.Status != "submitted" && application.Status != "resubmitted")
        {
            return Conflict(new { error = "Für diesen Antrag kann aktuell keine Rückfrage erstellt werden" });
        }

        var existingOpen = await _repository.GetLatestOpenInquiry(id);
        if (existingOpen != null)
        {
            return Conflict(new { error = "Es existiert bereits eine offene Rückfrage für diesen Antrag" });
        }

        var inquiry = await _repository.CreateApplicationInquiry(id, dto.InquiryText, User.GetEmail());
        await _repository.UpdateApplicationStatus(id, "needs_information");

        return StatusCode(201, inquiry);
    }

    [HttpPost("api/applications/{id:int}/inquiry/response")]
    [Authorize(Policy = AuthPolicies.Applicant)]
    public async Task<IActionResult> RespondToInquiry(int id, [FromBody] InquiryResponseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ResponseText))
        {
            return BadRequest(new { errors = new { responseText = new[] { "Antworttext darf nicht leer sein" } } });
        }

        var application = await _repository.GetApplicationById(id);
        if (application == null)
        {
            return NotFound(new { error = "Antrag nicht gefunden" });
        }

        if (application.CreatedBy != User.GetEmail())
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        if (application.Status != "needs_information")
        {
            return Conflict(new { error = "Für diesen Antrag kann aktuell keine Rückfrage beantwortet werden" });
        }

        var openInquiry = await _repository.GetLatestOpenInquiry(id);
        if (openInquiry == null)
        {
            return Conflict(new { error = "Es existiert keine offene Rückfrage für diesen Antrag" });
        }

        var inquiry = await _repository.AnswerApplicationInquiry(openInquiry.Id, dto.ResponseText);
        if (inquiry == null)
        {
            return StatusCode(500, new { error = "Die Rückfrageantwort konnte nicht gespeichert werden" });
        }

        await _repository.UpdateApplicationStatus(id, "resubmitted");

        return Ok(inquiry);
    }

    // Alternative endpoint used by frontend component
    [HttpPost("api/applications/{id:int}/answer-inquiry")]
    [Authorize(Policy = AuthPolicies.Applicant)]
    public async Task<IActionResult> AnswerInquiry(int id, [FromBody] InquiryResponseDto dto)
    {
        return await RespondToInquiry(id, dto);
    }

    // Processor inquiry endpoint (used by processor detail page)
    [HttpPost("api/processor/{id:int}/inquire")]
    [Authorize(Policy = AuthPolicies.Processor)]
    public async Task<IActionResult> ProcessorInquire(int id, [FromBody] InquiryCreateDto dto)
    {
        return await CreateInquiry(id, dto);
    }
}
