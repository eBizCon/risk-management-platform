using FluentValidation;
using MassTransit;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Application.Commands;

public record CreateApplicationCommand(ApplicationCreateDto Dto, string UserEmail) : ICommand<CreateApplicationResult>;

public record CreateApplicationResult(ApplicationResponse Application);

public class CreateApplicationHandler : ICommandHandler<CreateApplicationCommand, CreateApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly IValidator<ApplicationCreateDto> _validator;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateApplicationHandler(
        IApplicationRepository repository,
        IValidator<ApplicationCreateDto> validator,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _validator = validator;
        _publishEndpoint = publishEndpoint;
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

        var application = ApplicationEntity.CreateProcessing(
            command.Dto.CustomerId,
            Money.Create((decimal)command.Dto.Income),
            Money.Create((decimal)command.Dto.FixedCosts),
            Money.CreatePositive((decimal)command.Dto.DesiredRate),
            Money.CreatePositive((decimal)command.Dto.LoanAmount),
            command.Dto.LoanTerm,
            EmailAddress.Create(command.UserEmail));

        await _repository.AddAsync(application, ct);

        await _publishEndpoint.Publish(new ApplicationCreationStarted(
            Guid.NewGuid(),
            application.Id.Value,
            command.Dto.CustomerId,
            command.Dto.Income,
            command.Dto.FixedCosts,
            command.Dto.DesiredRate,
            command.Dto.LoanAmount,
            command.Dto.LoanTerm,
            command.UserEmail,
            false), ct);

        await _repository.SaveChangesAsync(ct);

        return Result<CreateApplicationResult>.Success(new CreateApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}
