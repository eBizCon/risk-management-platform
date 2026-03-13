using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Data;
using RiskManagement.Api.Extensions;
using RiskManagement.Api.Models;
using RiskManagement.Api.Validation;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly ApplicationRepository _repository;
    private readonly ApplicationValidator _createValidator;
    private readonly ApplicationUpdateValidator _updateValidator;

    public ApplicationsController(
        ApplicationRepository repository,
        ApplicationValidator createValidator,
        ApplicationUpdateValidator updateValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetApplications([FromQuery] string? status)
    {
        if (User.GetRole() != "applicant")
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        var applications = await _repository.GetApplicationsByUser(User.GetEmail(), status);
        return Ok(applications);
    }

    [HttpPost]
    public async Task<IActionResult> CreateApplication([FromBody] ApplicationCreateDto dto)
    {
        if (User.GetRole() != "applicant")
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        var validationResult = await _createValidator.ValidateAsync(dto);
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

        var application = await _repository.CreateApplication(new Application
        {
            Name = dto.Name,
            Income = dto.Income,
            FixedCosts = dto.FixedCosts,
            DesiredRate = dto.DesiredRate,
            EmploymentStatus = dto.EmploymentStatus,
            HasPaymentDefault = dto.HasPaymentDefault,
            Status = "draft",
            CreatedBy = User.GetEmail()
        });

        if (dto.Action == "submit" && application != null)
        {
            var submitted = await _repository.SubmitApplication(application.Id);
            if (submitted != null)
            {
                return Ok(new { application = submitted, redirect = $"/applications/{submitted.Id}?submitted=true" });
            }
        }

        return Ok(new { application, redirect = $"/applications/{application!.Id}" });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetApplication(int id)
    {
        var application = await _repository.GetApplicationById(id);
        if (application == null)
        {
            return NotFound(new { error = "Antrag nicht gefunden" });
        }

        var role = User.GetRole();
        if (role == "applicant" && application.CreatedBy != User.GetEmail())
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        if (role != "applicant" && role != "processor")
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        return Ok(application);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateApplication(int id, [FromBody] ApplicationUpdateDto dto)
    {
        if (User.GetRole() != "applicant")
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        var existing = await _repository.GetApplicationById(id);
        if (existing == null)
        {
            return NotFound(new { error = "Antrag nicht gefunden" });
        }

        if (existing.CreatedBy != User.GetEmail())
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        var validationResult = await _updateValidator.ValidateAsync(dto);
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

        var application = await _repository.UpdateApplication(id, dto);
        if (application == null)
        {
            return NotFound(new { error = "Antrag nicht gefunden oder kann nicht bearbeitet werden" });
        }

        if (dto.Action == "submit")
        {
            var submitted = await _repository.SubmitApplication(id);
            if (submitted != null)
            {
                return Ok(new { application = submitted, redirect = $"/applications/{id}?submitted=true" });
            }
        }

        return Ok(new { application, redirect = $"/applications/{id}" });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteApplication(int id)
    {
        if (User.GetRole() != "applicant")
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        var existing = await _repository.GetApplicationById(id);
        if (existing == null)
        {
            return NotFound(new { error = "Antrag nicht gefunden" });
        }

        if (existing.CreatedBy != User.GetEmail())
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        var success = await _repository.DeleteApplication(id);
        if (!success)
        {
            return BadRequest(new { error = "Antrag konnte nicht gelöscht werden (nur Entwürfe können gelöscht werden)" });
        }

        return Ok(new { success = true });
    }

    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> SubmitApplication(int id)
    {
        if (User.GetRole() != "applicant")
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        var existing = await _repository.GetApplicationById(id);
        if (existing == null)
        {
            return NotFound(new { error = "Antrag nicht gefunden" });
        }

        if (existing.CreatedBy != User.GetEmail())
        {
            return StatusCode(403, new { error = "Keine Berechtigung" });
        }

        var application = await _repository.SubmitApplication(id);
        if (application == null)
        {
            return BadRequest(new { error = "Antrag konnte nicht eingereicht werden (nur Entwürfe können eingereicht werden)" });
        }

        return Ok(application);
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
