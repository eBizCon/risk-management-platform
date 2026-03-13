using RiskManagement.Domain.Common;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Domain.Events;

public sealed class ApplicationDeletedEvent : IDomainEvent
{
    public AppId ApplicationId { get; }
    public DateTime OccurredOn { get; }

    public ApplicationDeletedEvent(AppId applicationId)
    {
        ApplicationId = applicationId;
        OccurredOn = DateTime.UtcNow;
    }
}