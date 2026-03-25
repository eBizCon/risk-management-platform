using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedKernel.Common;
using SharedKernel.Dispatching;

namespace SharedKernel.Persistence;

public class DomainEventDispatchInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatchInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken ct = default)
    {
        if (eventData.Context is null)
            return result;

        // IDispatcher aus dem Scope holen (nicht im Constructor, wegen Scoped Lifetime)
        var dispatcher = _serviceProvider.GetRequiredService<IDispatcher>();

        var aggregates = eventData.Context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        foreach (var aggregate in aggregates)
        {
            var events = aggregate.DomainEvents.ToList();
            aggregate.ClearDomainEvents();
            foreach (var evt in events)
                await dispatcher.PublishAsync(evt, ct);
        }

        return result;
    }
}
