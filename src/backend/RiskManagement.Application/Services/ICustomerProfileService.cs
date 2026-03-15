namespace RiskManagement.Application.Services;

public interface ICustomerProfileService
{
    Task<CustomerProfile?> GetCustomerProfileAsync(int customerId, CancellationToken ct = default);
}

public record CustomerProfile(
    int Id,
    string FirstName,
    string LastName,
    string EmploymentStatus,
    CustomerCreditReport? CreditReport,
    string Status);

public record CustomerCreditReport(
    bool HasPaymentDefault,
    int? CreditScore,
    string CheckedAt,
    string Provider);
