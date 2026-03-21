using MassTransit;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Infrastructure.Sagas.Consumers;

public class MarkApplicationFailedConsumer : IConsumer<MarkApplicationFailed>
{
    private readonly IApplicationRepository _repository;

    public MarkApplicationFailedConsumer(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<MarkApplicationFailed> context)
    {
        var msg = context.Message;

        var application = await _repository.GetByIdAsync(
            new AppId(msg.ApplicationId), context.CancellationToken);

        if (application is null)
            return;

        if (application.Status != ApplicationStatus.Processing)
            return;

        application.MarkFailed(msg.Reason);
        await _repository.SaveChangesAsync(context.CancellationToken);
    }
}
