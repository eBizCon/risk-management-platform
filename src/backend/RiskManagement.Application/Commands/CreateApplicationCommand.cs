using FluentValidation;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Application.Commands;

public record CreateApplicationCommand(ApplicationCreateDto Dto, string UserEmail);

public record CreateApplicationResult(ApplicationResponse Application, string Redirect);

public class CreateApplicationHandler : ICommandHandler<CreateApplicationCommand, CreateApplicationResult>
{
    private readonly IApplicationRepository _repository;
    private readonly ScoringService _scoringService;
    private readonly IValidator<ApplicationCreateDto> _validator;

    public CreateApplicationHandler(
        IApplicationRepository repository,
        ScoringService scoringService,
        IValidator<ApplicationCreateDto> validator)
    {
        _repository = repository;
        _scoringService = scoringService;
        _validator = validator;
    }

    public async Task<Result<CreateApplicationResult>> HandleAsync(CreateApplicationCommand command, CancellationToken ct = default)
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
            return Result<CreateApplicationResult>.ValidationFailure(errors, command.Dto);
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

        if (command.Dto.Action == "submit")
        {
            application.Submit(_scoringService);
            await _repository.SaveChangesAsync(ct);

            return Result<CreateApplicationResult>.Success(new CreateApplicationResult(
                ApplicationMapper.ToResponse(application),
                $"/applications/{application.Id}?submitted=true"));
        }

        return Result<CreateApplicationResult>.Success(new CreateApplicationResult(
            ApplicationMapper.ToResponse(application),
            $"/applications/{application.Id}"));
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
