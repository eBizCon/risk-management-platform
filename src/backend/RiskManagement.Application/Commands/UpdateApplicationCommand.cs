using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Application.Commands;

public record UpdateApplicationCommand(int ApplicationId, ApplicationUpdateDto Dto, string UserEmail);

public record UpdateApplicationResult(ApplicationResponse Application, string Redirect);

public class UpdateApplicationHandler : ICommandHandler<UpdateApplicationCommand, UpdateApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly ScoringService _scoringService;
    private readonly IValidator<ApplicationUpdateDto> _validator;

    public UpdateApplicationHandler(
        IApplicationRepository repository,
        ScoringService scoringService,
        IValidator<ApplicationUpdateDto> validator)
    {
        _repository = repository;
        _scoringService = scoringService;
        _validator = validator;
    }

    public async Task<Result<UpdateApplicationResult>> HandleAsync(UpdateApplicationCommand command, CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(command.ApplicationId, ct);
        if (application is null)
            return Result<UpdateApplicationResult>.NotFound("Antrag nicht gefunden");

        if (application.CreatedBy != command.UserEmail)
            return Result<UpdateApplicationResult>.Forbidden("Zugriff verweigert");

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
            return Result<UpdateApplicationResult>.ValidationFailure(errors, command.Dto);
        }

        application.UpdateDetails(
            command.Dto.Name,
            command.Dto.Income,
            command.Dto.FixedCosts,
            command.Dto.DesiredRate,
            EmploymentStatus.From(command.Dto.EmploymentStatus),
            command.Dto.HasPaymentDefault,
            _scoringService);

        if (command.Dto.Action == "submit")
        {
            application.Submit(_scoringService);
            await _repository.SaveChangesAsync(ct);

            return Result<UpdateApplicationResult>.Success(new UpdateApplicationResult(
                ApplicationMapper.ToResponse(application),
                $"/applications/{application.Id}?submitted=true"));
        }

        await _repository.SaveChangesAsync(ct);

        return Result<UpdateApplicationResult>.Success(new UpdateApplicationResult(
            ApplicationMapper.ToResponse(application),
            $"/applications/{application.Id}"));
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
