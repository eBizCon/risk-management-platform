using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;

namespace RiskManagement.Application.Commands;

public record ApproveApplicationCommand(int ApplicationId, ApproveApplicationDto Dto)
    : ICommand<ApproveApplicationResult>;

public record ApproveApplicationResult(ApplicationResponse Application, string Redirect);

public class ApproveApplicationHandler : ICommandHandler<ApproveApplicationCommand, ApproveApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly IValidator<ApproveApplicationDto> _validator;
    private readonly IDispatcher _dispatcher;

    public ApproveApplicationHandler(IApplicationRepository repository, IValidator<ApproveApplicationDto> validator,
        IDispatcher dispatcher)
    {
        _repository = repository;
        _validator = validator;
        _dispatcher = dispatcher;
    }

    public async Task<Result<ApproveApplicationResult>> HandleAsync(ApproveApplicationCommand command,
        CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = ValidationHelper.ToValidationErrors(validationResult);
            return Result<ApproveApplicationResult>.ValidationFailure(errors, command.Dto);
        }

        var application = await _repository.GetByIdAsync(command.ApplicationId, ct);
        if (application is null)
            return Result<ApproveApplicationResult>.NotFound("Antrag nicht gefunden");

        application.Approve(command.Dto.Comment);
        await _repository.SaveChangesAsync(ct);

        await _dispatcher.PublishDomainEventsAsync(application, ct);

        return Result<ApproveApplicationResult>.Success(new ApproveApplicationResult(
            ApplicationMapper.ToResponse(application),
            $"/processor/{application.Id}"));
    }
}