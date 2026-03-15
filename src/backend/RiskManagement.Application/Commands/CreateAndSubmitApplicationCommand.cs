using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Services;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Application.Commands;

public record CreateAndSubmitApplicationCommand(ApplicationCreateDto Dto, string UserEmail)
    : ICommand<CreateAndSubmitApplicationResult>;

public record CreateAndSubmitApplicationResult(ApplicationResponse Application);

public class
    CreateAndSubmitApplicationHandler : ICommandHandler<CreateAndSubmitApplicationCommand,
    CreateAndSubmitApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly IScoringConfigRepository _configRepository;
    private readonly IScoringService _scoringService;
    private readonly IValidator<ApplicationCreateDto> _validator;
    private readonly IDispatcher _dispatcher;
    private readonly ICustomerProfileService _customerProfileService;

    public CreateAndSubmitApplicationHandler(
        IApplicationRepository repository,
        IScoringConfigRepository configRepository,
        IScoringService scoringService,
        IValidator<ApplicationCreateDto> validator,
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

    public async Task<Result<CreateAndSubmitApplicationResult>> HandleAsync(CreateAndSubmitApplicationCommand command,
        CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = ValidationHelper.ToValidationErrors(validationResult);
            return Result<CreateAndSubmitApplicationResult>.ValidationFailure(errors, command.Dto);
        }

        var customerProfile = await _customerProfileService.GetCustomerProfileAsync(command.Dto.CustomerId, ct);
        if (customerProfile is null)
            return Result<CreateAndSubmitApplicationResult>.Failure("Kunde nicht gefunden");

        if (customerProfile.CreditReport is null)
            return Result<CreateAndSubmitApplicationResult>.Failure("Bonitätsprüfung fehlt. Bitte zuerst eine Bonitätsprüfung für den Kunden durchführen.");

        var configVersion = await _configRepository.GetCurrentAsync(ct);
        if (configVersion is null)
            return Result<CreateAndSubmitApplicationResult>.Failure("Keine Scoring-Konfiguration gefunden");

        var application = ApplicationEntity.Create(
            command.Dto.CustomerId,
            Money.Create((decimal)command.Dto.Income),
            Money.Create((decimal)command.Dto.FixedCosts),
            Money.CreatePositive((decimal)command.Dto.DesiredRate),
            EmploymentStatus.From(customerProfile.EmploymentStatus),
            customerProfile.CreditReport.HasPaymentDefault,
            customerProfile.CreditReport.CreditScore,
            EmailAddress.Create(command.UserEmail),
            _scoringService,
            configVersion.Config,
            configVersion.Id);

        await _repository.AddAsync(application, ct);
        await _repository.SaveChangesAsync(ct);

        application.Submit(_scoringService, configVersion.Config, configVersion.Id);
        await _repository.SaveChangesAsync(ct);

        await _dispatcher.PublishDomainEventsAsync(application, ct);

        return Result<CreateAndSubmitApplicationResult>.Success(new CreateAndSubmitApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}