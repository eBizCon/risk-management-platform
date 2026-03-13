using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;

namespace RiskManagement.Application.DTOs;

public static class ScoringConfigMapper
{
    public static ScoringConfigResponse ToResponse(ScoringConfigVersion version)
    {
        return new ScoringConfigResponse(
            Id: version.Id.Value,
            Version: version.Version,
            GreenThreshold: version.Config.GreenThreshold,
            YellowThreshold: version.Config.YellowThreshold,
            IncomeRatioGood: version.Config.IncomeRatioGood,
            IncomeRatioModerate: version.Config.IncomeRatioModerate,
            IncomeRatioLimited: version.Config.IncomeRatioLimited,
            PenaltyModerateRatio: version.Config.PenaltyModerateRatio,
            PenaltyLimitedRatio: version.Config.PenaltyLimitedRatio,
            PenaltyCriticalRatio: version.Config.PenaltyCriticalRatio,
            RateGood: version.Config.RateGood,
            RateModerate: version.Config.RateModerate,
            RateHeavy: version.Config.RateHeavy,
            PenaltyModerateRate: version.Config.PenaltyModerateRate,
            PenaltyHeavyRate: version.Config.PenaltyHeavyRate,
            PenaltyExcessiveRate: version.Config.PenaltyExcessiveRate,
            PenaltySelfEmployed: version.Config.PenaltySelfEmployed,
            PenaltyRetired: version.Config.PenaltyRetired,
            PenaltyUnemployed: version.Config.PenaltyUnemployed,
            PenaltyPaymentDefault: version.Config.PenaltyPaymentDefault,
            CreatedBy: version.CreatedBy.Value,
            CreatedAt: version.CreatedAt);
    }
}
