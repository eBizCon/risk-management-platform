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

public record CreateAndSubmitApplicationCommand(ApplicationCreateDto Dto, string UserEmail)
    : ICommand<CreateAndSubmitApplicationResult>;

public record CreateAndSubmitApplicationResult(ApplicationResponse Application);

public class
    CreateAndSubmitApplicationHandler : ICommandHandler<CreateAndSubmitApplicationCommand,
    CreateAndSubmitApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly IValidator<ApplicationCreateDto> _validator;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateAndSubmitApplicationHandler(
        IApplicationRepository repository,
        IValidator<ApplicationCreateDto> validator,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _validator = validator;
        _publishEndpoint = publishEndpoint;
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

        var application = ApplicationEntity.CreateProcessing(
            command.Dto.CustomerId,
            Money.Create((decimal)command.Dto.Income),
            Money.Create((decimal)command.Dto.FixedCosts),
            Money.CreatePositive((decimal)command.Dto.DesiredRate),
            EmailAddress.Create(command.UserEmail));

        await _repository.AddAsync(application, ct);
        await _repository.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(new ApplicationCreationStarted(
            Guid.NewGuid(),
            application.Id.Value,
            command.Dto.CustomerId,
            command.Dto.Income,
            command.Dto.FixedCosts,
            command.Dto.DesiredRate,
            command.UserEmail,
            true), ct);

        return Result<CreateAndSubmitApplicationResult>.Success(new CreateAndSubmitApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}