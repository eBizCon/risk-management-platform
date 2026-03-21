using CustomerManagement.Domain.Events;
using MassTransit;
using SharedKernel.Dispatching;
using SharedKernel.IntegrationEvents;

namespace CustomerManagement.Infrastructure.IntegrationEventPublishers;

public class CustomerArchivedIntegrationEventPublisher : IDomainEventHandler<CustomerArchivedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public CustomerArchivedIntegrationEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task HandleAsync(CustomerArchivedEvent domainEvent, CancellationToken ct = default)
    {
        await _publishEndpoint.Publish(
            new CustomerArchivedIntegrationEvent(domainEvent.CustomerId.Value),
            ct);
    }
}