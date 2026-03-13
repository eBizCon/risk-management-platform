using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;

namespace RiskManagement.Application.Commands;

public record RejectApplicationCommand(int ApplicationId, RejectApplicationDto Dto);

public record RejectApplicationResult(ApplicationResponse Application, string Redirect);

public class RejectApplicationHandler : ICommandHandler<RejectApplicationCommand, RejectApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly IValidator<RejectApplicationDto> _validator;

    public RejectApplicationHandler(IApplicationRepository repository, IValidator<RejectApplicationDto> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<RejectApplicationResult>> HandleAsync(RejectApplicationCommand command, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = ValidationHelper.ToValidationErrors(validationResult);
            return Result<RejectApplicationResult>.ValidationFailure(errors, command.Dto);
        }

        var application = await _repository.GetByIdAsync(command.ApplicationId, ct);
        if (application is null)
            return Result<RejectApplicationResult>.NotFound("Antrag nicht gefunden");

        application.Reject(command.Dto.Comment);
        await _repository.SaveChangesAsync(ct);

        return Result<RejectApplicationResult>.Success(new RejectApplicationResult(
            ApplicationMapper.ToResponse(application),
            $"/processor/{application.Id}"));
    }
}
