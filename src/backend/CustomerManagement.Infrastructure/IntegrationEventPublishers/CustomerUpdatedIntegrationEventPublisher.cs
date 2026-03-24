using CustomerManagement.Domain.Events;
using MassTransit;
using SharedKernel.Dispatching;
using SharedKernel.IntegrationEvents;

namespace CustomerManagement.Infrastructure.IntegrationEventPublishers;

public class CustomerUpdatedIntegrationEventPublisher : IDomainEventHandler<CustomerUpdatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public CustomerUpdatedIntegrationEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task HandleAsync(CustomerUpdatedEvent domainEvent, CancellationToken ct = default)
    {
        await _publishEndpoint.Publish(
            new CustomerUpdatedIntegrationEvent(
                domainEvent.CustomerId.Value,
                domainEvent.FirstName,
                domainEvent.LastName,
                domainEvent.Status),
            ct);
    }
}