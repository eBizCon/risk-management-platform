using CustomerManagement.Domain.Events;
using MassTransit;
using SharedKernel.Dispatching;
using SharedKernel.IntegrationEvents;

namespace CustomerManagement.Infrastructure.IntegrationEventPublishers;

public class CustomerCreatedIntegrationEventPublisher : IDomainEventHandler<CustomerCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public CustomerCreatedIntegrationEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task HandleAsync(CustomerCreatedEvent domainEvent, CancellationToken ct = default)
    {
        await _publishEndpoint.Publish(
            new CustomerCreatedIntegrationEvent(
                domainEvent.CustomerId.Value,
                domainEvent.FirstName,
                domainEvent.LastName,
                "active"),
            ct);
    }
}