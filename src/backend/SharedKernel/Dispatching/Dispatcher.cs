using System.Collections;
using SharedKernel.Common;
using SharedKernel.Results;

namespace SharedKernel.Dispatching;

public class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<Result<TResult>> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default)
    {
        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));
        var handler = _serviceProvider.GetService(handlerType)
                      ?? throw new InvalidOperationException($"No handler registered for {handlerType.Name}");

        var method = handlerType.GetMethod("HandleAsync")!;
        return (Task<Result<TResult>>)method.Invoke(handler, new object[] { command, ct })!;
    }

    public Task<Result<TResult>> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default)
    {
        var queryType = query.GetType();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));
        var handler = _serviceProvider.GetService(handlerType)
                      ?? throw new InvalidOperationException($"No handler registered for {handlerType.Name}");

        var method = handlerType.GetMethod("HandleAsync")!;
        return (Task<Result<TResult>>)method.Invoke(handler, new object[] { query, ct })!;
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var enumerableType = typeof(IEnumerable<>).MakeGenericType(handlerType);

        if (_serviceProvider.GetService(enumerableType) is not IEnumerable handlers)
            return;

        var method = handlerType.GetMethod("HandleAsync")!;
        foreach (var handler in handlers) await (Task)method.Invoke(handler, new object[] { domainEvent, ct })!;
    }

    public async Task PublishDomainEventsAsync(IHasDomainEvents aggregate, CancellationToken ct = default)
    {
        var events = aggregate.DomainEvents.ToList();
        aggregate.ClearDomainEvents();

        foreach (var domainEvent in events)
            await PublishAsync(domainEvent, ct);
    }
}
