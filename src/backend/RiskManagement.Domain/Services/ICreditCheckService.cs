using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Domain.Services;

public interface ICreditCheckService
{
    Task<CreditCheckResult> CheckAsync(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string street,
        string city,
        string zipCode,
        string country);
}
