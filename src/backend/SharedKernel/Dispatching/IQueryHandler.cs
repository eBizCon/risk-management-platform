using SharedKernel.Results;

namespace SharedKernel.Dispatching;

public interface IQueryHandler<in TQuery, TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken ct = default);
}