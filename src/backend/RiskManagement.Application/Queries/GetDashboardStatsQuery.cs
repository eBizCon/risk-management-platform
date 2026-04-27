using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;

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
        var (draft, submitted, approved, rejected) = query.Role == "processor"
            ? await _repository.GetDashboardStatsAsync(ct)
            : await _repository.GetDashboardStatsByUserAsync(EmailAddress.Create(query.UserEmail), ct);

        var dto = new DashboardStatsDto
        {
            Draft = draft,
            Submitted = submitted,
            Approved = approved,
            Rejected = rejected,
            Total = draft + submitted + approved + rejected
        };

        return Result<DashboardStatsDto>.Success(dto);
    }
}
