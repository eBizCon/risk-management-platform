using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Common;
using RiskManagement.Domain.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Domain.Events;

public sealed class ApplicationCreationRequestedEvent : IDomainEvent
{
    public AppId ApplicationId { get; }
    public int CustomerId { get; }
    public Money Income { get; }
    public Money FixedCosts { get; }
    public Money DesiredRate { get; }
    public EmailAddress CreatedBy { get; }
    public bool AutoSubmit { get; }
    public DateTime OccurredOn { get; }

    public ApplicationCreationRequestedEvent(
        AppId applicationId,
        int customerId,
        Money income,
        Money fixedCosts,
        Money desiredRate,
        EmailAddress createdBy,
        bool autoSubmit)
    {
        ApplicationId = applicationId;
        CustomerId = customerId;
        Income = income;
        FixedCosts = fixedCosts;
        DesiredRate = desiredRate;
        CreatedBy = createdBy;
        AutoSubmit = autoSubmit;
        OccurredOn = DateTime.UtcNow;
    }
}
