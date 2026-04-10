using MassTransit;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Domain.Events;
using SharedKernel.Dispatching;

namespace RiskManagement.Infrastructure.IntegrationEventPublishers;

public class ApplicationCreationRequestedEventHandler : IDomainEventHandler<ApplicationCreationRequestedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public ApplicationCreationRequestedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task HandleAsync(ApplicationCreationRequestedEvent domainEvent, CancellationToken ct = default)
    {
        await _publishEndpoint.Publish(
            new ApplicationCreationStarted(
                Guid.NewGuid(),
                domainEvent.ApplicationId.Value,
                domainEvent.CustomerId,
                (double)domainEvent.Income.Amount,
                (double)domainEvent.FixedCosts.Amount,
                (double)domainEvent.DesiredRate.Amount,
                domainEvent.CreatedBy.Value,
                domainEvent.AutoSubmit),
            ct);
    }
}
