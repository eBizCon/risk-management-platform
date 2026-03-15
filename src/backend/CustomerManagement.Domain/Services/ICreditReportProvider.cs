using CustomerManagement.Domain.ValueObjects;

namespace CustomerManagement.Domain.Services;

public interface ICreditReportProvider
{
    Task<CreditReport> CheckAsync(string firstName, string lastName, DateOnly dateOfBirth, Address address);
}
