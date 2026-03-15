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

public record UpdateAndSubmitApplicationCommand(int ApplicationId, ApplicationUpdateDto Dto, string UserEmail)
    : ICommand<UpdateAndSubmitApplicationResult>;

public record UpdateAndSubmitApplicationResult(ApplicationResponse Application);

public class
    UpdateAndSubmitApplicationHandler : ICommandHandler<UpdateAndSubmitApplicationCommand,
    UpdateAndSubmitApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly IScoringConfigRepository _configRepository;
    private readonly IScoringService _scoringService;
    private readonly IValidator<ApplicationUpdateDto> _validator;
    private readonly IDispatcher _dispatcher;
    private readonly ICustomerProfileService _customerProfileService;

    public UpdateAndSubmitApplicationHandler(
        IApplicationRepository repository,
        IScoringConfigRepository configRepository,
        IScoringService scoringService,
        IValidator<ApplicationUpdateDto> validator,
        IDispatcher dispatcher,
        ICustomerProfileService customerProfileService)
    {
        _repository = repository;
        _configRepository = configRepository;
        _scoringService = scoringService;
        _validator = validator;
        _dispatcher = dispatcher;
        _customerProfileService = customerProfileService;
    }

    public async Task<Result<UpdateAndSubmitApplicationResult>> HandleAsync(UpdateAndSubmitApplicationCommand command,
        CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(new AppId(command.ApplicationId), ct);
        if (application is null)
            return Result<UpdateAndSubmitApplicationResult>.NotFound("Antrag nicht gefunden");

        if (application.CreatedBy != EmailAddress.Create(command.UserEmail))
            return Result<UpdateAndSubmitApplicationResult>.Forbidden("Zugriff verweigert");

        var validationResult = await _validator.ValidateAsync(command.Dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = ValidationHelper.ToValidationErrors(validationResult);
            return Result<UpdateAndSubmitApplicationResult>.ValidationFailure(errors, command.Dto);
        }

        var customerProfile = await _customerProfileService.GetCustomerProfileAsync(command.Dto.CustomerId, ct);
        if (customerProfile is null)
            return Result<UpdateAndSubmitApplicationResult>.Failure("Kunde nicht gefunden");

        if (customerProfile.CreditReport is null)
            return Result<UpdateAndSubmitApplicationResult>.Failure("Bonitätsprüfung fehlt. Bitte zuerst eine Bonitätsprüfung für den Kunden durchführen.");

        var configVersion = await _configRepository.GetCurrentAsync(ct);
        if (configVersion is null)
            return Result<UpdateAndSubmitApplicationResult>.Failure("Keine Scoring-Konfiguration gefunden");

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

        application.Submit(_scoringService, configVersion.Config, configVersion.Id);
        await _repository.SaveChangesAsync(ct);

        await _dispatcher.PublishDomainEventsAsync(application, ct);

        return Result<UpdateAndSubmitApplicationResult>.Success(new UpdateAndSubmitApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}