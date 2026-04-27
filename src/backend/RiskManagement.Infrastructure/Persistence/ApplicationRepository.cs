using Microsoft.EntityFrameworkCore;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Infrastructure.Persistence;

public class ApplicationRepository : IApplicationRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationEntity?> GetByIdAsync(AppId id, CancellationToken ct = default)
    {
        return await _context.Applications
            .Include(a => a.Inquiries)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<ApplicationEntity?> GetByIdWithInquiriesAsync(AppId id, CancellationToken ct = default)
    {
        return await _context.Applications
            .Include(a => a.Inquiries)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<List<ApplicationEntity>> GetByUserAsync(EmailAddress userEmail, ApplicationStatus? status = null,
        CancellationToken ct = default)
    {
        var query = _context.Applications.Where(a => a.CreatedBy == userEmail);

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

    public async Task<(int Total, int Submitted, int Approved, int Rejected)> GetProcessorStatsAsync(
        CancellationToken ct = default)
    {
        var total = await _context.Applications.CountAsync(ct);
        var submitted = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Submitted, ct);
        var approved = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Approved, ct);
        var rejected = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Rejected, ct);

        return (total, submitted, approved, rejected);
    }

    public async Task<List<ApplicationInquiry>> GetInquiriesAsync(AppId applicationId,
        CancellationToken ct = default)
    {
        return await _context.ApplicationInquiries
            .Where(i => i.ApplicationId == applicationId)
            .OrderBy(i => i.CreatedAt)
            .ThenBy(i => i.Id)
            .ToListAsync(ct);
    }

    public async Task<ApplicationInquiry?> GetInquiryByIdAsync(InquiryId inquiryId, CancellationToken ct = default)
    {
        return await _context.ApplicationInquiries
            .FirstOrDefaultAsync(i => i.Id == inquiryId, ct);
    }

    public async Task<bool> HasOpenInquiryAsync(AppId applicationId, CancellationToken ct = default)
    {
        return await _context.ApplicationInquiries
            .AnyAsync(i => i.ApplicationId == applicationId && i.Status == InquiryStatus.Open, ct);
    }

    public async Task AddInquiryAsync(ApplicationInquiry inquiry, CancellationToken ct = default)
    {
        await _context.ApplicationInquiries.AddAsync(inquiry, ct);
    }

    public async Task<List<ApplicationEntity>> GetOpenApplicationsAsync(CancellationToken ct = default)
    {
        return await _context.Applications
            .Where(a => a.Status == ApplicationStatus.Submitted
                        || a.Status == ApplicationStatus.Resubmitted
                        || a.Status == ApplicationStatus.NeedsInformation)
            .ToListAsync(ct);
    }

    public async Task<List<ApplicationEntity>> GetApplicationsForExportAsync(ApplicationStatus? status = null,
        CancellationToken ct = default)
    {
        var query = _context.Applications.AsQueryable();
        if (status is not null)
            query = query.Where(a => a.Status == status);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
    }

    public async Task<bool> ExistsForCustomerAsync(int customerId, CancellationToken ct = default)
    {
        return await _context.Applications.AnyAsync(a => a.CustomerId == customerId, ct);
    }

    public async Task<(int Draft, int Submitted, int Approved, int Rejected)> GetDashboardStatsAsync(
        CancellationToken ct = default)
    {
        var draft = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Draft, ct);
        var submitted = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Submitted, ct);
        var approved = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Approved, ct);
        var rejected = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Rejected, ct);

        return (draft, submitted, approved, rejected);
    }

    public async Task<(int Draft, int Submitted, int Approved, int Rejected)> GetDashboardStatsByUserAsync(
        EmailAddress userEmail, CancellationToken ct = default)
    {
        var userApps = _context.Applications.Where(a => a.CreatedBy == userEmail);

        var draft = await userApps.CountAsync(a => a.Status == ApplicationStatus.Draft, ct);
        var submitted = await userApps.CountAsync(a => a.Status == ApplicationStatus.Submitted, ct);
        var approved = await userApps.CountAsync(a => a.Status == ApplicationStatus.Approved, ct);
        var rejected = await userApps.CountAsync(a => a.Status == ApplicationStatus.Rejected, ct);

        return (draft, submitted, approved, rejected);
    }
}