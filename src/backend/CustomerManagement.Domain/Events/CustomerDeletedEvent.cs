using CustomerManagement.Domain.Aggregates.CustomerAggregate;

namespace CustomerManagement.Domain.Events;

public record CustomerDeletedEvent(CustomerId CustomerId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
