using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Services;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;
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
    private readonly ICustomerProfileService _customerProfileService;

    public UpdateApplicationHandler(
        IApplicationRepository repository,
        IScoringConfigRepository configRepository,
        IScoringService scoringService,
        IValidator<ApplicationUpdateDto> validator,
        ICustomerProfileService customerProfileService)
    {
        _repository = repository;
        _configRepository = configRepository;
        _scoringService = scoringService;
        _validator = validator;
        _customerProfileService = customerProfileService;
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

        var customerProfile = await _customerProfileService.GetCustomerProfileAsync(command.Dto.CustomerId, ct);
        if (customerProfile is null)
            return Result<UpdateApplicationResult>.Failure("Kunde nicht gefunden");

        if (customerProfile.CreditReport is null)
            return Result<UpdateApplicationResult>.Failure("Bonit\u00e4tspr\u00fcfung fehlt. Bitte zuerst eine Bonit\u00e4tspr\u00fcfung f\u00fcr den Kunden durchf\u00fchren.");

        var configVersion = await _configRepository.GetCurrentAsync(ct);
        if (configVersion is null)
            return Result<UpdateApplicationResult>.Failure("Keine Scoring-Konfiguration gefunden");

        application.UpdateDetails(
            command.Dto.CustomerId,
            Money.Create((decimal)command.Dto.Income),
            Money.Create((decimal)command.Dto.FixedCosts),
            Money.CreatePositive((decimal)command.Dto.DesiredRate),
            EmploymentStatus.From(customerProfile.EmploymentStatus),
            customerProfile.CreditReport.HasPaymentDefault,
            customerProfile.CreditReport.CreditScore,
            _scoringService,
            configVersion.Config,
            configVersion.Id);

        await _repository.SaveChangesAsync(ct);

        return Result<UpdateApplicationResult>.Success(new UpdateApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}