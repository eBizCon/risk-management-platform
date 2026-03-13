using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Application.Commands;

public record RejectApplicationCommand(int ApplicationId, RejectApplicationDto Dto) : ICommand<RejectApplicationResult>;

public record RejectApplicationResult(ApplicationResponse Application);

public class RejectApplicationHandler : ICommandHandler<RejectApplicationCommand, RejectApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly IValidator<RejectApplicationDto> _validator;
    private readonly IDispatcher _dispatcher;

    public RejectApplicationHandler(IApplicationRepository repository, IValidator<RejectApplicationDto> validator,
        IDispatcher dispatcher)
    {
        _repository = repository;
        _validator = validator;
        _dispatcher = dispatcher;
    }

    public async Task<Result<RejectApplicationResult>> HandleAsync(RejectApplicationCommand command,
        CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = ValidationHelper.ToValidationErrors(validationResult);
            return Result<RejectApplicationResult>.ValidationFailure(errors, command.Dto);
        }

        var application = await _repository.GetByIdAsync(new AppId(command.ApplicationId), ct);
        if (application is null)
            return Result<RejectApplicationResult>.NotFound("Antrag nicht gefunden");

        application.Reject(command.Dto.Comment);
        await _repository.SaveChangesAsync(ct);

        await _dispatcher.PublishDomainEventsAsync(application, ct);

        return Result<RejectApplicationResult>.Success(new RejectApplicationResult(
            ApplicationMapper.ToResponse(application)));
    }
}