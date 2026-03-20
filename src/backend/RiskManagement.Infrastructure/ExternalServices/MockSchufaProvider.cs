using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Infrastructure.ExternalServices;

public class MockSchufaProvider : ICreditCheckService
{
    public Task<CreditCheckResult> CheckAsync(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string street,
        string city,
        string zipCode,
        string country)
    {
        var hasPaymentDefault = false;
        var creditScore = 420;

        if (lastName.Contains("Verzug", StringComparison.OrdinalIgnoreCase) ||
            lastName.Contains("Default", StringComparison.OrdinalIgnoreCase))
        {
            hasPaymentDefault = true;
            creditScore = 250;
        }
        else
        {
            var age = CalculateAge(dateOfBirth);
            if (age > 65)
                creditScore = 520;
            else if (age < 25)
                creditScore = 350;
        }

        var result = CreditCheckResult.Create(hasPaymentDefault, creditScore, DateTime.UtcNow, "schufa_mock");
        return Task.FromResult(result);
    }

    private static int CalculateAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age))
            age--;
        return age;
    }
}
