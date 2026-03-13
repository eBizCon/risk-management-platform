using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Data;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Models;
using RiskManagement.Api.Validation;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Route("api/processor")]
[Authorize]
public class ProcessorController : ControllerBase
{
    private readonly ApplicationRepository _repository;
    private readonly ProcessorDecisionValidator _decisionValidator;

    public ProcessorController(ApplicationRepository repository, ProcessorDecisionValidator decisionValidator)
    {
        _repository = repository;
        _decisionValidator = decisionValidator;
    }

    private static readonly string[] AllowedStatuses =
        { "submitted", "needs_information", "resubmitted", "approved", "rejected", "draft" };

    [HttpGet("applications")]
    public async Task<IActionResult> GetApplications([FromQuery] string? status, [FromQuery] int? page)
    {
        if (User.GetRole() != "processor")
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        var statusFilter = status != null && AllowedStatuses.Contains(status) ? status : null;
        var rawPage = page ?? 1;
        var safePage = rawPage > 0 ? rawPage : 1;

        var initialResult = await _repository.GetProcessorApplicationsPaginated(statusFilter, safePage, ApplicationRepository.PageSize);
        var totalPages = Math.Max(1, (int)Math.Ceiling((double)initialResult.TotalCount / ApplicationRepository.PageSize));
        var currentPage = Math.Min(Math.Max(safePage, 1), totalPages);

        var result = currentPage == safePage
            ? initialResult
            : await _repository.GetProcessorApplicationsPaginated(statusFilter, currentPage, ApplicationRepository.PageSize);

        var stats = await _repository.GetProcessorApplicationStats();

        return Ok(new ProcessorApplicationsResponse
        {
            Applications = result.Items.ToArray(),
            StatusFilter = statusFilter,
            Stats = stats,
            Pagination = new PaginationInfo
            {
                Page = currentPage,
                PageSize = ApplicationRepository.PageSize,
                TotalItems = result.TotalCount,
                TotalPages = totalPages
            }
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetApplication(int id)
    {
        if (User.GetRole() != "processor")
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        var application = await _repository.GetApplicationById(id);
        if (application == null)
        {
            return NotFound(new { error = "Antrag nicht gefunden" });
        }

        return Ok(application);
    }

    [HttpPost("{id:int}/decide")]
    public async Task<IActionResult> ProcessDecision(int id, [FromBody] ProcessorDecisionDto dto)
    {
        if (User.GetRole() != "processor")
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        var validationResult = await _decisionValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = new Dictionary<string, string[]>();
            foreach (var failure in validationResult.Errors)
            {
                var key = ToCamelCase(failure.PropertyName);
                if (!errors.ContainsKey(key))
                {
                    errors[key] = Array.Empty<string>();
                }
                errors[key] = errors[key].Append(failure.ErrorMessage).ToArray();
            }
            return BadRequest(new ValidationErrorResponse { Errors = errors, Values = dto });
        }

        var application = await _repository.ProcessApplication(id, dto.Decision, dto.Comment);
        if (application == null)
        {
            return BadRequest(new { error = "Antrag kann nicht bearbeitet werden (möglicherweise nicht im Status \"Eingereicht\" oder \"Erneut eingereicht\")" });
        }

        return Ok(new { application, redirect = $"/processor/{id}?processed=true" });
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
