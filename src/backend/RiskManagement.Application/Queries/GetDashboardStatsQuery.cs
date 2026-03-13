using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;

namespace RiskManagement.Application.Queries;

public record GetDashboardStatsQuery(string? UserEmail, string UserRole);

public class GetDashboardStatsHandler : IQueryHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IApplicationRepository _repository;

    public GetDashboardStatsHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DashboardStatsDto>> HandleAsync(GetDashboardStatsQuery query, CancellationToken ct = default)
    {
        var email = query.UserRole == "processor" ? null : query.UserEmail;
        var stats = await _repository.GetDashboardStatsAsync(email, ct);

        return Result<DashboardStatsDto>.Success(new DashboardStatsDto
        {
            Draft = stats.Draft,
            Submitted = stats.Submitted,
            Approved = stats.Approved,
            Rejected = stats.Rejected
        });
    }
}
