using MassTransit;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Domain.Events;
using SharedKernel.Dispatching;

namespace RiskManagement.Infrastructure.IntegrationEventPublishers;

public class ApplicationUpdateRequestedEventHandler : IDomainEventHandler<ApplicationUpdateRequestedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public ApplicationUpdateRequestedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task HandleAsync(ApplicationUpdateRequestedEvent domainEvent, CancellationToken ct = default)
    {
        await _publishEndpoint.Publish(
            new ApplicationUpdateStarted(
                Guid.NewGuid(),
                domainEvent.ApplicationId.Value,
                domainEvent.CustomerId,
                (double)domainEvent.Income.Amount,
                (double)domainEvent.FixedCosts.Amount,
                (double)domainEvent.DesiredRate.Amount,
                domainEvent.UserEmail.Value,
                domainEvent.AutoSubmit),
            ct);
    }
}
