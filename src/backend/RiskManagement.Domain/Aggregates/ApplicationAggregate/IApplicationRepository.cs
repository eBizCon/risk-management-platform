using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Domain.Aggregates.ApplicationAggregate;

public interface IApplicationRepository
{
    Task<Application?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Application>> GetByUserAsync(string email, ApplicationStatus? status = null, CancellationToken ct = default);
    Task<(List<Application> Items, int TotalCount)> GetAllPaginatedAsync(ApplicationStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Application application, CancellationToken ct = default);
    Task RemoveAsync(Application application, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<ProcessorStats> GetProcessorStatsAsync(CancellationToken ct = default);
    Task<DashboardStats> GetDashboardStatsAsync(string? userEmail = null, CancellationToken ct = default);
    Task<List<Application>> GetApplicationsForExportAsync(ApplicationStatus? status = null, CancellationToken ct = default);
    Task<List<ApplicationInquiry>> GetInquiriesAsync(int applicationId, CancellationToken ct = default);
}

public class ProcessorStats
{
    public int Total { get; set; }
    public int Submitted { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
}

public class DashboardStats
{
    public int Draft { get; set; }
    public int Submitted { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
}
