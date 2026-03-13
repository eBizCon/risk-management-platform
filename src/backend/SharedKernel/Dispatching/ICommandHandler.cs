using SharedKernel.Results;

namespace SharedKernel.Dispatching;

public interface ICommandHandler<in TCommand, TResult>
{
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken ct = default);
}
