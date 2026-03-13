using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Common;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Domain.Events;

public sealed class InquiryCreatedEvent : IDomainEvent
{
    public AppId ApplicationId { get; }
    public InquiryId InquiryId { get; }
    public DateTime OccurredOn { get; }

    public InquiryCreatedEvent(AppId applicationId, InquiryId inquiryId)
    {
        ApplicationId = applicationId;
        InquiryId = inquiryId;
        OccurredOn = DateTime.UtcNow;
    }
}