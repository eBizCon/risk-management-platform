using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Domain.ReadModels;

public interface IDashboardStatsQuery
{
    Task<(int Total, int Draft, int Submitted, int Approved, int Rejected)> GetStatsAsync(
        EmailAddress? userEmail = null, CancellationToken ct = default);
}
