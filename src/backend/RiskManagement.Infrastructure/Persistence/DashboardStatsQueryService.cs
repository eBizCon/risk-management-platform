using Microsoft.EntityFrameworkCore;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ReadModels;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Infrastructure.Persistence;

public class DashboardStatsQueryService : IDashboardStatsQuery
{
    private readonly ApplicationDbContext _dbContext;

    public DashboardStatsQueryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(int Total, int Draft, int Submitted, int Approved, int Rejected)> GetStatsAsync(
        EmailAddress? userEmail = null, CancellationToken ct = default)
    {
        var query = _dbContext.Applications.AsQueryable();

        if (userEmail is not null)
            query = query.Where(a => a.CreatedBy == userEmail);

        var total = await query.CountAsync(ct);
        var draft = await query.CountAsync(a => a.Status == ApplicationStatus.Draft, ct);
        var submitted = await query.CountAsync(a => a.Status == ApplicationStatus.Submitted, ct);
        var approved = await query.CountAsync(a => a.Status == ApplicationStatus.Approved, ct);
        var rejected = await query.CountAsync(a => a.Status == ApplicationStatus.Rejected, ct);

        return (total, draft, submitted, approved, rejected);
    }
}
