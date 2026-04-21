using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;

namespace RiskManagement.Application.DTOs;

public static class ScoringConfigMapper
{
    public static ScoringConfigResponse ToResponse(ScoringConfigVersion version)
    {
        return new ScoringConfigResponse(
            version.Id.Value,
            version.Version,
            version.Config.GreenThreshold,
            version.Config.YellowThreshold,
            version.Config.IncomeRatioGood,
            version.Config.IncomeRatioModerate,
            version.Config.IncomeRatioLimited,
            version.Config.PenaltyModerateRatio,
            version.Config.PenaltyLimitedRatio,
            version.Config.PenaltyCriticalRatio,
            version.Config.RateGood,
            version.Config.RateModerate,
            version.Config.RateHeavy,
            version.Config.PenaltyModerateRate,
            version.Config.PenaltyHeavyRate,
            version.Config.PenaltyExcessiveRate,
            version.Config.PenaltySelfEmployed,
            version.Config.PenaltyRetired,
            version.Config.PenaltyUnemployed,
            version.Config.PenaltyPaymentDefault,
            version.Config.CreditScoreGood,
            version.Config.CreditScoreModerate,
            version.Config.PenaltyModerateCreditScore,
            version.Config.PenaltyLowCreditScore,
            version.Config.LoanToIncomeRatioGood,
            version.Config.LoanToIncomeRatioModerate,
            version.Config.LoanToIncomeRatioHigh,
            version.Config.PenaltyModerateLoanToIncome,
            version.Config.PenaltyHighLoanToIncome,
            version.Config.PenaltyCriticalLoanToIncome,
            version.Config.LoanTermShort,
            version.Config.LoanTermMedium,
            version.Config.LoanTermLong,
            version.Config.PenaltyMediumLoanTerm,
            version.Config.PenaltyLongLoanTerm,
            version.Config.PenaltyVeryLongLoanTerm,
            version.CreatedBy.Value,
            version.CreatedAt);
    }
}
