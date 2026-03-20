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

public record CreateApplicationCommand(ApplicationCreateDto Dto, string UserEmail) : ICommand<CreateApplicationResult>;

public record CreateApplicationResult(ApplicationResponse Application);

public class CreateApplicationHandler : ICommandHandler<CreateApplicationCommand, CreateApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly IScoringConfigRepository _configRepository;
    private readonly IScoringService _scoringService;
    private readonly IValidator<ApplicationCreateDto> _validator;
    private readonly ICustomerProfileService _customerProfileService;
    private readonly ICreditCheckService _creditCheckService;

    public CreateApplicationHandler(
        IApplicationRepository repository,
        IScoringConfigRepository configRepository,
        IScoringService scoringService,
        IValidator<ApplicationCreateDto> validator,
        ICustomerProfileService customerProfileService,
        ICreditCheckService creditCheckService)
    {
        _repository = repository;
        _configRepository = configRepository;
        _scoringService = scoringService;
        _validator = validator;
        _customerProfileService = customerProfileService;
        _creditCheckService = creditCheckService;
    }

    public async Task<Result<CreateApplicationResult>> HandleAsync(CreateApplicationCommand command,
        CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = ValidationHelper.ToValidationErrors(validationResult);
            return Result<CreateApplicationResult>.ValidationFailure(errors, command.Dto);
        }

        var customerProfile = await _customerProfileService.GetCustomerProfileAsync(command.Dto.CustomerId, ct);
        if (customerProfile is null)
            return Result<CreateApplicationResult>.Failure("Kunde nicht gefunden");

        var checkResult = await _creditCheckService.CheckAsync(
            customerProfile.FirstName,
            customerProfile.LastName,
            DateOnly.Parse(customerProfile.DateOfBirth),
            customerProfile.Address.Street,
            customerProfile.Address.City,
            customerProfile.Address.ZipCode,
            customerProfile.Address.Country);
        var creditReport = CreditReport.FromCheckResult(checkResult);

        var configVersion = await _configRepository.GetCurrentAsync(ct);
        if (configVersion is null)
            return Result<CreateApplicationResult>.Failure("Keine Scoring-Konfiguration gefunden");

        var application = ApplicationEntity.Create(
            command.Dto.CustomerId,
            Money.Create((decimal)command.Dto.Income),
            Money.Create((decimal)command.Dto.FixedCosts),
            Money.CreatePositive((decimal)command.Dto.DesiredRate),
            EmploymentStatus.From(customerProfile.EmploymentStatus),
            creditReport,
            EmailAddress.Create(command.UserEmail),
            _scoringService,
            configVersion.Config,
            configVersion.Id);

        await _repository.AddAsync(application, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<CreateApplicationResult>.Success(new CreateApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}