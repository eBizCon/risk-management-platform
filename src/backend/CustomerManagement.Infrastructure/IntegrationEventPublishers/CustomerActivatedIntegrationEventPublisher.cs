using CustomerManagement.Domain.Events;
using MassTransit;
using SharedKernel.Dispatching;
using SharedKernel.IntegrationEvents;

namespace CustomerManagement.Infrastructure.IntegrationEventPublishers;

public class CustomerActivatedIntegrationEventPublisher : IDomainEventHandler<CustomerActivatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public CustomerActivatedIntegrationEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task HandleAsync(CustomerActivatedEvent domainEvent, CancellationToken ct = default)
    {
        await _publishEndpoint.Publish(
            new CustomerActivatedIntegrationEvent(domainEvent.CustomerId.Value),
            ct);
    }
}