using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Application.Queries;

public record GetDashboardStatsQuery(string? UserEmail) : IQuery<DashboardStatsDto>;

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
        EmailAddress? userEmail = query.UserEmail is not null
            ? EmailAddress.Create(query.UserEmail)
            : null;

        var (draft, submitted, approved, rejected) =
            await _repository.GetDashboardStatsAsync(userEmail, ct);

        return Result<DashboardStatsDto>.Success(new DashboardStatsDto
        {
            Draft = draft,
            Submitted = submitted,
            Approved = approved,
            Rejected = rejected
        });
    }
}
