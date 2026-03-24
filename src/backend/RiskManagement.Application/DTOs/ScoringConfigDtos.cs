namespace RiskManagement.Application.DTOs;

public record ScoringConfigResponse(
    int Id,
    int Version,
    int GreenThreshold,
    int YellowThreshold,
    decimal IncomeRatioGood,
    decimal IncomeRatioModerate,
    decimal IncomeRatioLimited,
    int PenaltyModerateRatio,
    int PenaltyLimitedRatio,
    int PenaltyCriticalRatio,
    decimal RateGood,
    decimal RateModerate,
    decimal RateHeavy,
    int PenaltyModerateRate,
    int PenaltyHeavyRate,
    int PenaltyExcessiveRate,
    int PenaltySelfEmployed,
    int PenaltyRetired,
    int PenaltyUnemployed,
    int PenaltyPaymentDefault,
    string CreatedBy,
    DateTime CreatedAt);

public record ScoringConfigUpdateDto(
    int GreenThreshold,
    int YellowThreshold,
    decimal IncomeRatioGood,
    decimal IncomeRatioModerate,
    decimal IncomeRatioLimited,
    int PenaltyModerateRatio,
    int PenaltyLimitedRatio,
    int PenaltyCriticalRatio,
    decimal RateGood,
    decimal RateModerate,
    decimal RateHeavy,
    int PenaltyModerateRate,
    int PenaltyHeavyRate,
    int PenaltyExcessiveRate,
    int PenaltySelfEmployed,
    int PenaltyRetired,
    int PenaltyUnemployed,
    int PenaltyPaymentDefault);

public record RescoreResult(int RescoredCount);