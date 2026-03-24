using CustomerManagement.Domain.Aggregates.CustomerAggregate;

namespace CustomerManagement.Domain.Events;

public record CustomerActivatedEvent(CustomerId CustomerId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}