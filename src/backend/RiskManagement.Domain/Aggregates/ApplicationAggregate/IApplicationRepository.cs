using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Domain.Aggregates.ApplicationAggregate;

public interface IApplicationRepository
{
    Task<Application?> GetByIdAsync(ApplicationId id, CancellationToken ct = default);
    Task<Application?> GetByIdWithInquiriesAsync(ApplicationId id, CancellationToken ct = default);

    Task<List<Application>> GetByUserAsync(EmailAddress userEmail, ApplicationStatus? status = null,
        CancellationToken ct = default);

    Task<(List<Application> Items, int TotalCount)> GetAllPaginatedAsync(ApplicationStatus? status, int page,
        int pageSize, CancellationToken ct = default);

    Task AddAsync(Application application, CancellationToken ct = default);
    Task RemoveAsync(Application application, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);

    Task<List<ApplicationInquiry>> GetInquiriesAsync(ApplicationId applicationId, CancellationToken ct = default);

    Task<(int Total, int Submitted, int Approved, int Rejected)> GetProcessorStatsAsync(
        CancellationToken ct = default);

    Task<ApplicationInquiry?> GetInquiryByIdAsync(InquiryId inquiryId, CancellationToken ct = default);

    Task<bool> HasOpenInquiryAsync(ApplicationId applicationId, CancellationToken ct = default);

    Task AddInquiryAsync(ApplicationInquiry inquiry, CancellationToken ct = default);

    Task<List<Application>> GetOpenApplicationsAsync(CancellationToken ct = default);

    Task<bool> ExistsForCustomerAsync(int customerId, CancellationToken ct = default);
}