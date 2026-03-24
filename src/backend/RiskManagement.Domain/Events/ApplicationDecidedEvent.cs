using RiskManagement.Domain.Common;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Domain.Events;

public sealed class ApplicationDecidedEvent : IDomainEvent
{
    public AppId ApplicationId { get; }
    public string Decision { get; }
    public DateTime OccurredOn { get; }

    public ApplicationDecidedEvent(AppId applicationId, string decision)
    {
        ApplicationId = applicationId;
        Decision = decision;
        OccurredOn = DateTime.UtcNow;
    }
}