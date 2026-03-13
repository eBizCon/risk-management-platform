using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Application.Commands;

public record UpdateApplicationCommand(int ApplicationId, ApplicationUpdateDto Dto, string UserEmail)
    : ICommand<UpdateApplicationResult>;

public record UpdateApplicationResult(ApplicationResponse Application);

public class UpdateApplicationHandler : ICommandHandler<UpdateApplicationCommand, UpdateApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly IScoringConfigRepository _configRepository;
    private readonly IScoringService _scoringService;
    private readonly IValidator<ApplicationUpdateDto> _validator;

    public UpdateApplicationHandler(
        IApplicationRepository repository,
        IScoringConfigRepository configRepository,
        IScoringService scoringService,
        IValidator<ApplicationUpdateDto> validator)
    {
        _repository = repository;
        _configRepository = configRepository;
        _scoringService = scoringService;
        _validator = validator;
    }

    public async Task<Result<UpdateApplicationResult>> HandleAsync(UpdateApplicationCommand command,
        CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(new AppId(command.ApplicationId), ct);
        if (application is null)
            return Result<UpdateApplicationResult>.NotFound("Antrag nicht gefunden");

        if (application.CreatedBy != EmailAddress.Create(command.UserEmail))
            return Result<UpdateApplicationResult>.Forbidden("Zugriff verweigert");

        var validationResult = await _validator.ValidateAsync(command.Dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = ValidationHelper.ToValidationErrors(validationResult);
            return Result<UpdateApplicationResult>.ValidationFailure(errors, command.Dto);
        }

        var configVersion = await _configRepository.GetCurrentAsync(ct);
        if (configVersion is null)
            return Result<UpdateApplicationResult>.Failure("Keine Scoring-Konfiguration gefunden");

        application.UpdateDetails(
            command.Dto.CustomerId,
            Money.Create((decimal)command.Dto.Income),
            Money.Create((decimal)command.Dto.FixedCosts),
            Money.CreatePositive((decimal)command.Dto.DesiredRate),
            EmploymentStatus.From(command.Dto.EmploymentStatus),
            command.Dto.HasPaymentDefault,
            _scoringService,
            configVersion.Config,
            configVersion.Id);

        await _repository.SaveChangesAsync(ct);

        return Result<UpdateApplicationResult>.Success(new UpdateApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}