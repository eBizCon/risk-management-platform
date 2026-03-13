using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
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
    private readonly IScoringService _scoringService;
    private readonly IValidator<ApplicationUpdateDto> _validator;
    private readonly IDispatcher _dispatcher;

    public UpdateAndSubmitApplicationHandler(
        IApplicationRepository repository,
        IScoringService scoringService,
        IValidator<ApplicationUpdateDto> validator,
        IDispatcher dispatcher)
    {
        _repository = repository;
        _scoringService = scoringService;
        _validator = validator;
        _dispatcher = dispatcher;
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

        application.UpdateDetails(
            command.Dto.Name,
            Money.Create((decimal)command.Dto.Income),
            Money.Create((decimal)command.Dto.FixedCosts),
            Money.CreatePositive((decimal)command.Dto.DesiredRate),
            EmploymentStatus.From(command.Dto.EmploymentStatus),
            command.Dto.HasPaymentDefault,
            _scoringService);

        application.Submit(_scoringService);
        await _repository.SaveChangesAsync(ct);

        await _dispatcher.PublishDomainEventsAsync(application, ct);

        return Result<UpdateAndSubmitApplicationResult>.Success(new UpdateAndSubmitApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}