using FluentValidation;
using MassTransit;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
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
    private readonly IValidator<ApplicationUpdateDto> _validator;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateAndSubmitApplicationHandler(
        IApplicationRepository repository,
        IValidator<ApplicationUpdateDto> validator,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _validator = validator;
        _publishEndpoint = publishEndpoint;
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

        application.SetProcessing();
        await _repository.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(new ApplicationUpdateStarted(
            Guid.NewGuid(),
            application.Id.Value,
            command.Dto.CustomerId,
            command.Dto.Income,
            command.Dto.FixedCosts,
            command.Dto.DesiredRate,
            command.UserEmail,
            true), ct);

        return Result<UpdateAndSubmitApplicationResult>.Success(new UpdateAndSubmitApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}