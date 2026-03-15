using CustomerManagement.Domain.Services;
using CustomerManagement.Domain.ValueObjects;

namespace CustomerManagement.Infrastructure.ExternalServices;

public class MockSchufaProvider : ICreditReportProvider
{
    public Task<CreditReport> CheckAsync(string firstName, string lastName, DateOnly dateOfBirth, Address address)
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

        var report = CreditReport.Create(hasPaymentDefault, creditScore, DateTime.UtcNow, "schufa_mock");
        return Task.FromResult(report);
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
