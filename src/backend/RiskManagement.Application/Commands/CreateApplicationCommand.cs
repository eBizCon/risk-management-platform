using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Application.Commands;

public record CreateApplicationCommand(ApplicationCreateDto Dto, string UserEmail) : ICommand<CreateApplicationResult>;

public record CreateApplicationResult(ApplicationResponse Application);

public class CreateApplicationHandler : ICommandHandler<CreateApplicationCommand, CreateApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly IScoringService _scoringService;
    private readonly IValidator<ApplicationCreateDto> _validator;

    public CreateApplicationHandler(
        IApplicationRepository repository,
        IScoringService scoringService,
        IValidator<ApplicationCreateDto> validator)
    {
        _repository = repository;
        _scoringService = scoringService;
        _validator = validator;
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

        var application = ApplicationEntity.Create(
            command.Dto.Name,
            Money.Create((decimal)command.Dto.Income),
            Money.Create((decimal)command.Dto.FixedCosts),
            Money.CreatePositive((decimal)command.Dto.DesiredRate),
            EmploymentStatus.From(command.Dto.EmploymentStatus),
            command.Dto.HasPaymentDefault,
            EmailAddress.Create(command.UserEmail),
            _scoringService);

        await _repository.AddAsync(application, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<CreateApplicationResult>.Success(new CreateApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}