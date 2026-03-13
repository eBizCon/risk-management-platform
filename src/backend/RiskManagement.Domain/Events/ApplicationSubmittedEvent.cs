using RiskManagement.Domain.Common;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Domain.Events;

public sealed class ApplicationSubmittedEvent : IDomainEvent
{
    public AppId ApplicationId { get; }
    public DateTime OccurredOn { get; }

    public ApplicationSubmittedEvent(AppId applicationId)
    {
        ApplicationId = applicationId;
        OccurredOn = DateTime.UtcNow;
    }
}