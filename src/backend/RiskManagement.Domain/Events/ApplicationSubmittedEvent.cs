using RiskManagement.Domain.Common;

namespace RiskManagement.Domain.Events;

public sealed class ApplicationSubmittedEvent : IDomainEvent
{
    public int ApplicationId { get; }
    public DateTime OccurredOn { get; }

    public ApplicationSubmittedEvent(int applicationId)
    {
        ApplicationId = applicationId;
        OccurredOn = DateTime.UtcNow;
    }
}