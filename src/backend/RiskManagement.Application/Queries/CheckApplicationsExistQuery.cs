using RiskManagement.Application.Common;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;

namespace RiskManagement.Application.Queries;

public record CheckApplicationsExistQuery(int CustomerId) : IQuery<CheckApplicationsExistResult>;

public record CheckApplicationsExistResult(bool Exists);

public class CheckApplicationsExistHandler : IQueryHandler<CheckApplicationsExistQuery, CheckApplicationsExistResult>
{
    private readonly IApplicationRepository _repository;

    public CheckApplicationsExistHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CheckApplicationsExistResult>> HandleAsync(CheckApplicationsExistQuery query,
        CancellationToken ct = default)
    {
        var exists = await _repository.ExistsForCustomerAsync(query.CustomerId, ct);
        return Result<CheckApplicationsExistResult>.Success(new CheckApplicationsExistResult(exists));
    }
}
