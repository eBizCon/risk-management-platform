using CustomerManagement.Domain.Aggregates.CustomerAggregate;

namespace CustomerManagement.Domain.Events;

public record CustomerCreatedEvent(CustomerId CustomerId, string FirstName, string LastName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
