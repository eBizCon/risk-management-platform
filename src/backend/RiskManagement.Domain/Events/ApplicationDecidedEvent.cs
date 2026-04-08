using RiskManagement.Domain.Common;
using RiskManagement.Domain.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Domain.Events;

public sealed class ApplicationDecidedEvent : IDomainEvent
{
    public AppId ApplicationId { get; }
    public DecisionType Decision { get; }
    public DateTime OccurredOn { get; }

    public ApplicationDecidedEvent(AppId applicationId, DecisionType decision)
    {
        ApplicationId = applicationId;
        Decision = decision;
        OccurredOn = DateTime.UtcNow;
    }
}
