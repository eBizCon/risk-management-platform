namespace RiskManagement.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
