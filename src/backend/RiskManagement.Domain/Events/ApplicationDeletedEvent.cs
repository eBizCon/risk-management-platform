using RiskManagement.Domain.Common;

namespace RiskManagement.Domain.Events;

public sealed class ApplicationDeletedEvent : IDomainEvent
{
    public int ApplicationId { get; }
    public DateTime OccurredOn { get; }

    public ApplicationDeletedEvent(int applicationId)
    {
        ApplicationId = applicationId;
        OccurredOn = DateTime.UtcNow;
    }
}
