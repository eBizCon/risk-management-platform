using SharedKernel.Common;

namespace SharedKernel.Dispatching;

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}
