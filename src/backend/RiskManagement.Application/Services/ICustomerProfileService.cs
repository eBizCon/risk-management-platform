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
    string DateOfBirth,
    CustomerAddress Address,
    string Status);

public record CustomerAddress(
    string Street,
    string City,
    string ZipCode,
    string Country);
