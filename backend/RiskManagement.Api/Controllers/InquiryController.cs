using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Data;
using RiskManagement.Api.Models;

namespace RiskManagement.Api.Controllers;

[ApiController]
public class InquiryController : ControllerBase
{
    private readonly ApplicationRepository _repository;

    public InquiryController(ApplicationRepository repository)
    {
        _repository = repository;
    }

    private UserSession? GetUser() => HttpContext.Items["User"] as UserSession;

    [HttpGet("api/applications/{id:int}/inquiries")]
    public async Task<IActionResult> GetInquiries(int id)
    {
        var user = GetUser();
        if (user == null)
        {
            return Unauthorized(new { error = "Login erforderlich" });
        }

        if (user.Role != "applicant" && user.Role != "processor")
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        var application = await _repository.GetApplicationById(id);
        if (application == null)
        {
            return NotFound(new { error = "Antrag nicht gefunden" });
        }

        if (user.Role == "applicant" && application.CreatedBy != user.Email)
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        var inquiries = await _repository.GetApplicationInquiries(id);
        return Ok(inquiries);
    }

    [HttpPost("api/applications/{id:int}/inquiry")]
    public async Task<IActionResult> CreateInquiry(int id, [FromBody] InquiryCreateDto dto)
    {
        var user = GetUser();
        if (user == null)
        {
            return Unauthorized(new { error = "Login erforderlich" });
        }

        if (user.Role != "processor")
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

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

        var inquiry = await _repository.CreateApplicationInquiry(id, dto.InquiryText, user.Email);
        await _repository.UpdateApplicationStatus(id, "needs_information");

        return StatusCode(201, inquiry);
    }

    [HttpPost("api/applications/{id:int}/inquiry/response")]
    public async Task<IActionResult> RespondToInquiry(int id, [FromBody] InquiryResponseDto dto)
    {
        var user = GetUser();
        if (user == null)
        {
            return Unauthorized(new { error = "Login erforderlich" });
        }

        if (user.Role != "applicant")
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        if (string.IsNullOrWhiteSpace(dto.ResponseText))
        {
            return BadRequest(new { errors = new { responseText = new[] { "Antworttext darf nicht leer sein" } } });
        }

        var application = await _repository.GetApplicationById(id);
        if (application == null)
        {
            return NotFound(new { error = "Antrag nicht gefunden" });
        }

        if (application.CreatedBy != user.Email)
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
    public async Task<IActionResult> AnswerInquiry(int id, [FromBody] InquiryResponseDto dto)
    {
        return await RespondToInquiry(id, dto);
    }

    // Processor inquiry endpoint (used by processor detail page)
    [HttpPost("api/processor/{id:int}/inquire")]
    public async Task<IActionResult> ProcessorInquire(int id, [FromBody] InquiryCreateDto dto)
    {
        return await CreateInquiry(id, dto);
    }
}
