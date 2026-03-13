using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Application.Commands;

public record UpdateApplicationCommand(int ApplicationId, ApplicationUpdateDto Dto, string UserEmail)
    : ICommand<UpdateApplicationResult>;

public record UpdateApplicationResult(ApplicationResponse Application);

public class UpdateApplicationHandler : ICommandHandler<UpdateApplicationCommand, UpdateApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly IScoringService _scoringService;
    private readonly IValidator<ApplicationUpdateDto> _validator;

    public UpdateApplicationHandler(
        IApplicationRepository repository,
        IScoringService scoringService,
        IValidator<ApplicationUpdateDto> validator)
    {
        _repository = repository;
        _scoringService = scoringService;
        _validator = validator;
    }

    public async Task<Result<UpdateApplicationResult>> HandleAsync(UpdateApplicationCommand command,
        CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(command.ApplicationId, ct);
        if (application is null)
            return Result<UpdateApplicationResult>.NotFound("Antrag nicht gefunden");

        if (application.CreatedBy != command.UserEmail)
            return Result<UpdateApplicationResult>.Forbidden("Zugriff verweigert");

        var validationResult = await _validator.ValidateAsync(command.Dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = ValidationHelper.ToValidationErrors(validationResult);
            return Result<UpdateApplicationResult>.ValidationFailure(errors, command.Dto);
        }

        application.UpdateDetails(
            command.Dto.Name,
            command.Dto.Income,
            command.Dto.FixedCosts,
            command.Dto.DesiredRate,
            EmploymentStatus.From(command.Dto.EmploymentStatus),
            command.Dto.HasPaymentDefault,
            _scoringService);

        await _repository.SaveChangesAsync(ct);

        return Result<UpdateApplicationResult>.Success(new UpdateApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}