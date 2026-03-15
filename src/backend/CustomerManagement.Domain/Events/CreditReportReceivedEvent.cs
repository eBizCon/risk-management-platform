using CustomerManagement.Domain.Aggregates.CustomerAggregate;

namespace CustomerManagement.Domain.Events;

public record CreditReportReceivedEvent(CustomerId CustomerId, bool HasPaymentDefault, int? CreditScore) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
