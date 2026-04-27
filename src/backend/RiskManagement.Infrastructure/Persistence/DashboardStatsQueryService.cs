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

        var groups = await query
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var total = groups.Sum(g => g.Count);
        var draft = groups.Where(g => g.Status == ApplicationStatus.Draft).Select(g => g.Count).FirstOrDefault();
        var submitted = groups.Where(g => g.Status == ApplicationStatus.Submitted).Select(g => g.Count).FirstOrDefault();
        var approved = groups.Where(g => g.Status == ApplicationStatus.Approved).Select(g => g.Count).FirstOrDefault();
        var rejected = groups.Where(g => g.Status == ApplicationStatus.Rejected).Select(g => g.Count).FirstOrDefault();

        return (total, draft, submitted, approved, rejected);
    }
}
