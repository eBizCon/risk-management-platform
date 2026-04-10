using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
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

    public CreateAndSubmitApplicationHandler(
        IApplicationRepository repository,
        IValidator<ApplicationCreateDto> validator)
    {
        _repository = repository;
        _validator = validator;
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
            EmailAddress.Create(command.UserEmail),
            true);

        await _repository.AddAsync(application, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<CreateAndSubmitApplicationResult>.Success(new CreateAndSubmitApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}
