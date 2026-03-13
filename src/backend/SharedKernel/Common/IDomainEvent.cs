namespace SharedKernel.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
