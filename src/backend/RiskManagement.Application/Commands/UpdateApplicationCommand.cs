using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
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
    private readonly IValidator<ApplicationUpdateDto> _validator;

    public UpdateApplicationHandler(
        IApplicationRepository repository,
        IValidator<ApplicationUpdateDto> validator)
    {
        _repository = repository;
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

        if (application.Status != ApplicationStatus.Draft)
            return Result<UpdateApplicationResult>.Failure("Nur Entwürfe können bearbeitet werden");

        var validationResult = await _validator.ValidateAsync(command.Dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = ValidationHelper.ToValidationErrors(validationResult);
            return Result<UpdateApplicationResult>.ValidationFailure(errors, command.Dto);
        }

        application.RequestProcessing(
            command.Dto.CustomerId,
            Money.Create((decimal)command.Dto.Income),
            Money.Create((decimal)command.Dto.FixedCosts),
            Money.CreatePositive((decimal)command.Dto.DesiredRate),
            EmailAddress.Create(command.UserEmail),
            false);

        await _repository.SaveChangesAsync(ct);

        return Result<UpdateApplicationResult>.Success(new UpdateApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}
