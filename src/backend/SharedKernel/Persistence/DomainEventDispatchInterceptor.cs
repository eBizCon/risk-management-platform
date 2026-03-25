using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedKernel.Common;
using SharedKernel.Dispatching;

namespace SharedKernel.Persistence;

public class DomainEventDispatchInterceptor : SaveChangesInterceptor
{
    private readonly IDispatcher _dispatcher;
    private List<IDomainEvent> _pendingEvents = [];

    public DomainEventDispatchInterceptor(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        if (eventData.Context is not null)
        {
            var aggregates = eventData.Context.ChangeTracker
                .Entries<IHasDomainEvents>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            foreach (var aggregate in aggregates)
            {
                _pendingEvents.AddRange(aggregate.DomainEvents);
                aggregate.ClearDomainEvents();
            }
        }

        return base.SavingChangesAsync(eventData, result, ct);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken ct = default)
    {
        var events = _pendingEvents.ToList();
        _pendingEvents.Clear();

        foreach (var evt in events)
            await _dispatcher.PublishAsync(evt, ct);

        return result;
    }
}
