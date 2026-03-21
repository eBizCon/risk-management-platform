using MassTransit;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;

namespace RiskManagement.Infrastructure.Sagas.Consumers;

public class FinalizeApplicationUpdateConsumer : IConsumer<FinalizeApplicationUpdate>
{
    private readonly IApplicationRepository _repository;
    private readonly IScoringConfigRepository _configRepository;
    private readonly IScoringService _scoringService;

    public FinalizeApplicationUpdateConsumer(
        IApplicationRepository repository,
        IScoringConfigRepository configRepository,
        IScoringService scoringService)
    {
        _repository = repository;
        _configRepository = configRepository;
        _scoringService = scoringService;
    }

    public async Task Consume(ConsumeContext<FinalizeApplicationUpdate> context)
    {
        var msg = context.Message;

        var application = await _repository.GetByIdAsync(
            new AppId(msg.ApplicationId), context.CancellationToken);

        if (application is null)
        {
            await context.Publish(new ApplicationCreationFailed(
                msg.CorrelationId,
                "Antrag nicht gefunden"));
            return;
        }

        try
        {
            var configVersion = await _configRepository.GetCurrentAsync(context.CancellationToken);
            if (configVersion is null)
            {
                await context.Publish(new ApplicationCreationFailed(
                    msg.CorrelationId,
                    "Keine Scoring-Konfiguration gefunden"));
                return;
            }

            var creditReport = CreditReport.Create(
                msg.HasPaymentDefault,
                msg.CreditScore,
                msg.CreditCheckedAt,
                msg.CreditProvider);

            application.UpdateDetails(
                msg.CustomerId,
                Money.Create(Convert.ToDecimal(msg.Income)),
                Money.Create(Convert.ToDecimal(msg.FixedCosts)),
                Money.Create(Convert.ToDecimal(msg.DesiredRate)),
                EmploymentStatus.From(msg.EmploymentStatus),
                creditReport,
                _scoringService,
                configVersion.Config,
                configVersion.Id);

            if (msg.AutoSubmit)
                application.Submit(
                    _scoringService,
                    configVersion.Config,
                    configVersion.Id);

            await _repository.SaveChangesAsync(context.CancellationToken);

            await context.Publish(new ApplicationCreationCompleted(msg.CorrelationId));
        }
        catch (Exception ex)
        {
            application.MarkFailed(ex.Message);
            await _repository.SaveChangesAsync(context.CancellationToken);

            await context.Publish(new ApplicationCreationFailed(
                msg.CorrelationId,
                ex.Message));
        }
    }
}
