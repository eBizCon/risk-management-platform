using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Application.Commands;

public record UpdateAndSubmitApplicationCommand(int ApplicationId, ApplicationUpdateDto Dto, string UserEmail);

public record UpdateAndSubmitApplicationResult(ApplicationResponse Application, string Redirect);

public class UpdateAndSubmitApplicationHandler : ICommandHandler<UpdateAndSubmitApplicationCommand, UpdateAndSubmitApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly ScoringService _scoringService;
    private readonly IValidator<ApplicationUpdateDto> _validator;

    public UpdateAndSubmitApplicationHandler(
        IApplicationRepository repository,
        ScoringService scoringService,
        IValidator<ApplicationUpdateDto> validator)
    {
        _repository = repository;
        _scoringService = scoringService;
        _validator = validator;
    }

    public async Task<Result<UpdateAndSubmitApplicationResult>> HandleAsync(UpdateAndSubmitApplicationCommand command, CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(command.ApplicationId, ct);
        if (application is null)
            return Result<UpdateAndSubmitApplicationResult>.NotFound("Antrag nicht gefunden");

        if (application.CreatedBy != command.UserEmail)
            return Result<UpdateAndSubmitApplicationResult>.Forbidden("Zugriff verweigert");

        var validationResult = await _validator.ValidateAsync(command.Dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = ValidationHelper.ToValidationErrors(validationResult);
            return Result<UpdateAndSubmitApplicationResult>.ValidationFailure(errors, command.Dto);
        }

        application.UpdateDetails(
            command.Dto.Name,
            command.Dto.Income,
            command.Dto.FixedCosts,
            command.Dto.DesiredRate,
            EmploymentStatus.From(command.Dto.EmploymentStatus),
            command.Dto.HasPaymentDefault,
            _scoringService);

        application.Submit(_scoringService);
        await _repository.SaveChangesAsync(ct);

        return Result<UpdateAndSubmitApplicationResult>.Success(new UpdateAndSubmitApplicationResult(
            ApplicationMapper.ToResponse(application),
            $"/applications/{application.Id}?submitted=true"));
    }
}
