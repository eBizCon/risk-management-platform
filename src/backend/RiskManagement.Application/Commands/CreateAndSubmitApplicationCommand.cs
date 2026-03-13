using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Application.Commands;

public record CreateAndSubmitApplicationCommand(ApplicationCreateDto Dto, string UserEmail)
    : ICommand<CreateAndSubmitApplicationResult>;

public record CreateAndSubmitApplicationResult(ApplicationResponse Application, string Redirect);

public class
    CreateAndSubmitApplicationHandler : ICommandHandler<CreateAndSubmitApplicationCommand,
    CreateAndSubmitApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly ScoringService _scoringService;
    private readonly IValidator<ApplicationCreateDto> _validator;
    private readonly IDispatcher _dispatcher;

    public CreateAndSubmitApplicationHandler(
        IApplicationRepository repository,
        ScoringService scoringService,
        IValidator<ApplicationCreateDto> validator,
        IDispatcher dispatcher)
    {
        _repository = repository;
        _scoringService = scoringService;
        _validator = validator;
        _dispatcher = dispatcher;
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

        var application = ApplicationEntity.Create(
            command.Dto.Name,
            command.Dto.Income,
            command.Dto.FixedCosts,
            command.Dto.DesiredRate,
            EmploymentStatus.From(command.Dto.EmploymentStatus),
            command.Dto.HasPaymentDefault,
            command.UserEmail,
            _scoringService);

        await _repository.AddAsync(application, ct);
        await _repository.SaveChangesAsync(ct);

        application.Submit(_scoringService);
        await _repository.SaveChangesAsync(ct);

        await _dispatcher.PublishDomainEventsAsync(application, ct);

        return Result<CreateAndSubmitApplicationResult>.Success(new CreateAndSubmitApplicationResult(
            ApplicationMapper.ToResponse(application),
            $"/applications/{application.Id}?submitted=true"));
    }
}