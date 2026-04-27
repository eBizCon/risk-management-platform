using RiskManagement.Application.DTOs;
using RiskManagement.Domain.ReadModels;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.Dispatching;
using SharedKernel.Results;

namespace RiskManagement.Application.Queries;

public record GetDashboardStatsQuery(string UserEmail, string Role) : IQuery<DashboardStatsDto>;

public class GetDashboardStatsHandler : IQueryHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IDashboardStatsQuery _dashboardStats;

    public GetDashboardStatsHandler(IDashboardStatsQuery dashboardStats)
    {
        _dashboardStats = dashboardStats;
    }

    public async Task<Result<DashboardStatsDto>> HandleAsync(GetDashboardStatsQuery query,
        CancellationToken ct = default)
    {
        var userEmail = query.Role == "applicant"
            ? EmailAddress.Create(query.UserEmail)
            : null;

        var (total, draft, submitted, approved, rejected) =
            await _dashboardStats.GetStatsAsync(userEmail, ct);

        var dto = new DashboardStatsDto
        {
            Total = total,
            Draft = draft,
            Submitted = submitted,
            Approved = approved,
            Rejected = rejected
        };

        return Result<DashboardStatsDto>.Success(dto);
    }
}
