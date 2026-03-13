using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Domain.Services;

public interface IScoringService
{
    ScoringResult CalculateScore(
        double income,
        double fixedCosts,
        double desiredRate,
        EmploymentStatus employmentStatus,
        bool hasPaymentDefault);
}
