using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;

namespace RiskManagement.Domain.Services;

public interface IScoringService
{
    ScoringResult CalculateScore(
        Money income,
        Money fixedCosts,
        Money desiredRate,
        EmploymentStatus employmentStatus,
        bool hasPaymentDefault,
        int? creditScore,
        ScoringConfig config);
}
