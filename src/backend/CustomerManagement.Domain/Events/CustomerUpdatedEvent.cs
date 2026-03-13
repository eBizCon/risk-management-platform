using CustomerManagement.Domain.Aggregates.CustomerAggregate;

namespace CustomerManagement.Domain.Events;

public record CustomerUpdatedEvent(CustomerId CustomerId, string FirstName, string LastName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}