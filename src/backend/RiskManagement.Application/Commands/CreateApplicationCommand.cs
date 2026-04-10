using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
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

    public CreateApplicationHandler(
        IApplicationRepository repository,
        IValidator<ApplicationCreateDto> validator)
    {
        _repository = repository;
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

        var application = ApplicationEntity.CreateProcessing(
            command.Dto.CustomerId,
            Money.Create((decimal)command.Dto.Income),
            Money.Create((decimal)command.Dto.FixedCosts),
            Money.CreatePositive((decimal)command.Dto.DesiredRate),
            EmailAddress.Create(command.UserEmail),
            false);

        await _repository.AddAsync(application, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<CreateApplicationResult>.Success(new CreateApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}
