using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;

namespace RiskManagement.Application.Commands;

public record ProcessDecisionCommand(int ApplicationId, ProcessorDecisionDto Dto);

public record ProcessDecisionResult(ApplicationResponse Application, string Redirect);

public class ProcessDecisionHandler : ICommandHandler<ProcessDecisionCommand, ProcessDecisionResult>
{
    private readonly IApplicationRepository _repository;
    private readonly IValidator<ProcessorDecisionDto> _validator;

    public ProcessDecisionHandler(IApplicationRepository repository, IValidator<ProcessorDecisionDto> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<ProcessDecisionResult>> HandleAsync(ProcessDecisionCommand command, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = new Dictionary<string, string[]>();
            foreach (var failure in validationResult.Errors)
            {
                var key = ToCamelCase(failure.PropertyName);
                if (!errors.ContainsKey(key))
                    errors[key] = Array.Empty<string>();
                errors[key] = errors[key].Append(failure.ErrorMessage).ToArray();
            }
            return Result<ProcessDecisionResult>.ValidationFailure(errors, command.Dto);
        }

        var application = await _repository.GetByIdAsync(command.ApplicationId, ct);
        if (application is null)
            return Result<ProcessDecisionResult>.NotFound("Antrag nicht gefunden");

        if (command.Dto.Decision == "approved")
            application.Approve(command.Dto.Comment);
        else
            application.Reject(command.Dto.Comment);

        await _repository.SaveChangesAsync(ct);

        return Result<ProcessDecisionResult>.Success(new ProcessDecisionResult(
            ApplicationMapper.ToResponse(application),
            $"/processor/{application.Id}"));
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
