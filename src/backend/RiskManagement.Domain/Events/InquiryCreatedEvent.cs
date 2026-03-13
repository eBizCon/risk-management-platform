using RiskManagement.Domain.Common;

namespace RiskManagement.Domain.Events;

public sealed class InquiryCreatedEvent : IDomainEvent
{
    public int ApplicationId { get; }
    public int InquiryId { get; }
    public DateTime OccurredOn { get; }

    public InquiryCreatedEvent(int applicationId, int inquiryId)
    {
        ApplicationId = applicationId;
        InquiryId = inquiryId;
        OccurredOn = DateTime.UtcNow;
    }
}