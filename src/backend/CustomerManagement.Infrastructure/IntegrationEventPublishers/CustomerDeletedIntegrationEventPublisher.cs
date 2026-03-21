using CustomerManagement.Domain.Events;
using MassTransit;
using SharedKernel.Dispatching;
using SharedKernel.IntegrationEvents;

namespace CustomerManagement.Infrastructure.IntegrationEventPublishers;

public class CustomerDeletedIntegrationEventPublisher : IDomainEventHandler<CustomerDeletedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public CustomerDeletedIntegrationEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task HandleAsync(CustomerDeletedEvent domainEvent, CancellationToken ct = default)
    {
        await _publishEndpoint.Publish(
            new CustomerDeletedIntegrationEvent(domainEvent.CustomerId.Value),
            ct);
    }
}