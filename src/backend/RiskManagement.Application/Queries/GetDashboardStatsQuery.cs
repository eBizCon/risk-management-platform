using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.Dispatching;
using SharedKernel.Results;

namespace RiskManagement.Application.Queries;

public record GetDashboardStatsQuery(string UserEmail, string Role) : IQuery<DashboardStatsDto>;

public class GetDashboardStatsHandler : IQueryHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IApplicationRepository _repository;

    public GetDashboardStatsHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DashboardStatsDto>> HandleAsync(GetDashboardStatsQuery query,
        CancellationToken ct = default)
    {
        var userEmail = query.Role == "applicant"
            ? EmailAddress.Create(query.UserEmail)
            : null;

        var (draft, submitted, approved, rejected) =
            await _repository.GetDashboardStatsAsync(userEmail, ct);

        var dto = new DashboardStatsDto
        {
            Total = draft + submitted + approved + rejected,
            Draft = draft,
            Submitted = submitted,
            Approved = approved,
            Rejected = rejected
        };

        return Result<DashboardStatsDto>.Success(dto);
    }
}
