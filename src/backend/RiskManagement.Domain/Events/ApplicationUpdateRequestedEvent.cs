using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Common;
using RiskManagement.Domain.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Domain.Events;

public sealed class ApplicationUpdateRequestedEvent : IDomainEvent
{
    public AppId ApplicationId { get; }
    public int CustomerId { get; }
    public Money Income { get; }
    public Money FixedCosts { get; }
    public Money DesiredRate { get; }
    public EmailAddress UserEmail { get; }
    public bool AutoSubmit { get; }
    public DateTime OccurredOn { get; }

    public ApplicationUpdateRequestedEvent(
        AppId applicationId,
        int customerId,
        Money income,
        Money fixedCosts,
        Money desiredRate,
        EmailAddress userEmail,
        bool autoSubmit)
    {
        ApplicationId = applicationId;
        CustomerId = customerId;
        Income = income;
        FixedCosts = fixedCosts;
        DesiredRate = desiredRate;
        UserEmail = userEmail;
        AutoSubmit = autoSubmit;
        OccurredOn = DateTime.UtcNow;
    }
}
