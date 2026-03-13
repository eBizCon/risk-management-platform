using Microsoft.EntityFrameworkCore;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Infrastructure.Persistence;

public class ApplicationRepository : IApplicationRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationEntity?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Applications
            .Include(a => a.Inquiries)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<List<ApplicationEntity>> GetByUserAsync(string email, ApplicationStatus? status = null,
        CancellationToken ct = default)
    {
        var query = _context.Applications.Where(a => a.CreatedBy == email);

        if (status is not null)
            query = query.Where(a => a.Status == status);

        return await query.ToListAsync(ct);
    }

    public async Task<(List<ApplicationEntity> Items, int TotalCount)> GetAllPaginatedAsync(ApplicationStatus? status,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Applications.AsQueryable();

        if (status is not null)
            query = query.Where(a => a.Status == status);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(ApplicationEntity application, CancellationToken ct = default)
    {
        await _context.Applications.AddAsync(application, ct);
    }

    public Task RemoveAsync(ApplicationEntity application, CancellationToken ct = default)
    {
        _context.Applications.Remove(application);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }

    public async Task<ProcessorStats> GetProcessorStatsAsync(CancellationToken ct = default)
    {
        var total = await _context.Applications.CountAsync(ct);
        var submitted = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Submitted, ct);
        var approved = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Approved, ct);
        var rejected = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Rejected, ct);

        return new ProcessorStats
        {
            Total = total,
            Submitted = submitted,
            Approved = approved,
            Rejected = rejected
        };
    }

    public async Task<DashboardStats> GetDashboardStatsAsync(string? userEmail = null, CancellationToken ct = default)
    {
        var query = _context.Applications.AsQueryable();
        if (!string.IsNullOrEmpty(userEmail))
            query = query.Where(a => a.CreatedBy == userEmail);

        var draft = await query.CountAsync(a => a.Status == ApplicationStatus.Draft, ct);
        var submitted = await query.CountAsync(a =>
            a.Status == ApplicationStatus.Submitted ||
            a.Status == ApplicationStatus.NeedsInformation ||
            a.Status == ApplicationStatus.Resubmitted, ct);
        var approved = await query.CountAsync(a => a.Status == ApplicationStatus.Approved, ct);
        var rejected = await query.CountAsync(a => a.Status == ApplicationStatus.Rejected, ct);

        return new DashboardStats
        {
            Draft = draft,
            Submitted = submitted,
            Approved = approved,
            Rejected = rejected
        };
    }

    public async Task<List<ApplicationEntity>> GetApplicationsForExportAsync(ApplicationStatus? status = null,
        CancellationToken ct = default)
    {
        var query = _context.Applications.AsQueryable();
        if (status is not null)
            query = query.Where(a => a.Status == status);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
    }

    public async Task<List<ApplicationInquiry>> GetInquiriesAsync(int applicationId, CancellationToken ct = default)
    {
        return await _context.ApplicationInquiries
            .Where(i => i.ApplicationId == applicationId)
            .OrderBy(i => i.CreatedAt)
            .ThenBy(i => i.Id)
            .ToListAsync(ct);
    }
}