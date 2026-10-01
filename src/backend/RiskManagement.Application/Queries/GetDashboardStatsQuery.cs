using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Application.Queries;

public record GetDashboardStatsQuery(string UserEmail, string UserRole) : IQuery<DashboardStatsDto>;

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
        EmailAddress? emailFilter = query.UserRole == "applicant"
            ? EmailAddress.Create(query.UserEmail)
            : null;

        var (draft, submitted, approved, rejected) = await _repository.GetDashboardStatsAsync(emailFilter, ct);

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
