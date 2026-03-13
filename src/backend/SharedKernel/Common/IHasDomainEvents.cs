namespace SharedKernel.Common;

public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
