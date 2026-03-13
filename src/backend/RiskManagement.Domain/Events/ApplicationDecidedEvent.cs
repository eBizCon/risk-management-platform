using RiskManagement.Domain.Common;

namespace RiskManagement.Domain.Events;

public sealed class ApplicationDecidedEvent : IDomainEvent
{
    public int ApplicationId { get; }
    public string Decision { get; }
    public DateTime OccurredOn { get; }

    public ApplicationDecidedEvent(int applicationId, string decision)
    {
        ApplicationId = applicationId;
        Decision = decision;
        OccurredOn = DateTime.UtcNow;
    }
}