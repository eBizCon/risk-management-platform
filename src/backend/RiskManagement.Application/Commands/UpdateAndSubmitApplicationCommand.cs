using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
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
    private readonly IValidator<ApplicationUpdateDto> _validator;

    public UpdateAndSubmitApplicationHandler(
        IApplicationRepository repository,
        IValidator<ApplicationUpdateDto> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<UpdateAndSubmitApplicationResult>> HandleAsync(UpdateAndSubmitApplicationCommand command,
        CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(new AppId(command.ApplicationId), ct);
        if (application is null)
            return Result<UpdateAndSubmitApplicationResult>.NotFound("Antrag nicht gefunden");

        if (application.CreatedBy != EmailAddress.Create(command.UserEmail))
            return Result<UpdateAndSubmitApplicationResult>.Forbidden("Zugriff verweigert");

        if (application.Status != ApplicationStatus.Draft)
            return Result<UpdateAndSubmitApplicationResult>.Failure("Nur Entwürfe können bearbeitet werden");

        var validationResult = await _validator.ValidateAsync(command.Dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = ValidationHelper.ToValidationErrors(validationResult);
            return Result<UpdateAndSubmitApplicationResult>.ValidationFailure(errors, command.Dto);
        }

        application.RequestProcessing(
            command.Dto.CustomerId,
            Money.Create((decimal)command.Dto.Income),
            Money.Create((decimal)command.Dto.FixedCosts),
            Money.CreatePositive((decimal)command.Dto.DesiredRate),
            EmailAddress.Create(command.UserEmail),
            true);

        await _repository.SaveChangesAsync(ct);

        return Result<UpdateAndSubmitApplicationResult>.Success(new UpdateAndSubmitApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}
