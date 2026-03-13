using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Domain.Services;

public interface IScoringService
{
    ScoringResult CalculateScore(
        Money income,
        Money fixedCosts,
        Money desiredRate,
        EmploymentStatus employmentStatus,
        bool hasPaymentDefault);
}
