using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Application.Commands;

public record SubmitApplicationCommand(int ApplicationId, string UserEmail) : ICommand<ApplicationResponse>;

public class SubmitApplicationHandler : ICommandHandler<SubmitApplicationCommand, ApplicationResponse>
{
    private readonly IApplicationRepository _repository;

    public SubmitApplicationHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ApplicationResponse>> HandleAsync(SubmitApplicationCommand command,
        CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(new AppId(command.ApplicationId), ct);
        if (application is null)
            return Result<ApplicationResponse>.NotFound("Antrag nicht gefunden");

        if (application.CreatedBy != EmailAddress.Create(command.UserEmail))
            return Result<ApplicationResponse>.Forbidden("Zugriff verweigert");

        if (application.Status != ApplicationStatus.Draft)
            return Result<ApplicationResponse>.Failure("Nur Entwürfe können eingereicht werden");

        application.RequestProcessing(
            application.CustomerId,
            application.Income,
            application.FixedCosts,
            application.DesiredRate,
            EmailAddress.Create(command.UserEmail),
            true);

        await _repository.SaveChangesAsync(ct);

        return Result<ApplicationResponse>.Success(ApplicationMapper.ToResponse(application));
    }
}
