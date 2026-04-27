using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;

namespace RiskManagement.Application.Queries;

public record GetDashboardStatsQuery(string UserEmail, string Role) : IQuery<ApplicationDashboardStatsDto>;

public class GetDashboardStatsHandler : IQueryHandler<GetDashboardStatsQuery, ApplicationDashboardStatsDto>
{
    private readonly IApplicationRepository _repository;

    public GetDashboardStatsHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ApplicationDashboardStatsDto>> HandleAsync(GetDashboardStatsQuery query,
        CancellationToken ct = default)
    {
        var (draft, submitted, approved, rejected) = query.Role == "processor"
            ? await _repository.GetDashboardStatsAsync(ct)
            : await _repository.GetUserDashboardStatsAsync(EmailAddress.Create(query.UserEmail), ct);

        var dto = new ApplicationDashboardStatsDto
        {
            Draft = draft,
            Submitted = submitted,
            Approved = approved,
            Rejected = rejected,
            Total = draft + submitted + approved + rejected
        };

        return Result<ApplicationDashboardStatsDto>.Success(dto);
    }
}
