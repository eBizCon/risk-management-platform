using MassTransit;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using SharedKernel.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Application.Commands;

public record SubmitApplicationCommand(int ApplicationId, string UserEmail) : ICommand<ApplicationResponse>;

public class SubmitApplicationHandler : ICommandHandler<SubmitApplicationCommand, ApplicationResponse>
{
    private readonly IApplicationRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public SubmitApplicationHandler(
        IApplicationRepository repository,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<ApplicationResponse>> HandleAsync(SubmitApplicationCommand command,
        CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(new AppId(command.ApplicationId), ct);
        if (application is null)
            return Result<ApplicationResponse>.NotFound("Antrag nicht gefunden");

        if (application.CreatedBy != EmailAddress.Create(command.UserEmail))
            return Result<ApplicationResponse>.Forbidden("Zugriff verweigert");

        if (application.Status != Domain.ValueObjects.ApplicationStatus.Draft)
            return Result<ApplicationResponse>.Failure("Nur Entwürfe können eingereicht werden");

        application.SetProcessing();

        await _publishEndpoint.Publish(new ApplicationUpdateStarted(
            Guid.NewGuid(),
            application.Id.Value,
            application.CustomerId,
            (double)application.Income.Amount,
            (double)application.FixedCosts.Amount,
            (double)application.DesiredRate.Amount,
            application.LoanAmount != null ? (double)application.LoanAmount.Amount : 0,
            application.LoanTerm ?? 0,
            command.UserEmail,
            true), ct);

        await _repository.SaveChangesAsync(ct);

        return Result<ApplicationResponse>.Success(ApplicationMapper.ToResponse(application));
    }
}
