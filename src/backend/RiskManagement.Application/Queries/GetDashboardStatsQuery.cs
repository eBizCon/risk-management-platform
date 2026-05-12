using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Application.Queries;

public record GetDashboardStatsQuery(string UserEmail, string Role) : IQuery<DashboardStatsResponse>;

public class GetDashboardStatsHandler : IQueryHandler<GetDashboardStatsQuery, DashboardStatsResponse>
{
    private readonly IApplicationRepository _repository;

    public GetDashboardStatsHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DashboardStatsResponse>> HandleAsync(GetDashboardStatsQuery query,
        CancellationToken ct = default)
    {
        EmailAddress? userEmail = query.Role == "applicant"
            ? EmailAddress.Create(query.UserEmail)
            : null;

        var (draft, submitted, approved, rejected) = await _repository.GetDashboardStatsAsync(userEmail, ct);

        var response = new DashboardStatsResponse
        {
            Total = draft + submitted + approved + rejected,
            Draft = draft,
            Submitted = submitted,
            Approved = approved,
            Rejected = rejected
        };

        return Result<DashboardStatsResponse>.Success(response);
    }
}
