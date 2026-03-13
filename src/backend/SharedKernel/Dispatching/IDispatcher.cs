using SharedKernel.Common;
using SharedKernel.Results;

namespace SharedKernel.Dispatching;

public interface IDispatcher
{
    Task<Result<TResult>> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default);
    Task<Result<TResult>> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default);
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default);
    Task PublishDomainEventsAsync(IHasDomainEvents aggregate, CancellationToken ct = default);
}
