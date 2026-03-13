using RiskManagement.Domain.Common;
using RiskManagement.Domain.Exceptions;

namespace RiskManagement.Domain.Aggregates.ScoringConfigAggregate;

public sealed class ScoringConfig : ValueObject
{
    public int GreenThreshold { get; }
    public int YellowThreshold { get; }

    public decimal IncomeRatioGood { get; }
    public decimal IncomeRatioModerate { get; }
    public decimal IncomeRatioLimited { get; }
    public int PenaltyModerateRatio { get; }
    public int PenaltyLimitedRatio { get; }
    public int PenaltyCriticalRatio { get; }

    public decimal RateGood { get; }
    public decimal RateModerate { get; }
    public decimal RateHeavy { get; }
    public int PenaltyModerateRate { get; }
    public int PenaltyHeavyRate { get; }
    public int PenaltyExcessiveRate { get; }

    public int PenaltySelfEmployed { get; }
    public int PenaltyRetired { get; }
    public int PenaltyUnemployed { get; }
    public int PenaltyPaymentDefault { get; }

    private ScoringConfig(
        int greenThreshold,
        int yellowThreshold,
        decimal incomeRatioGood,
        decimal incomeRatioModerate,
        decimal incomeRatioLimited,
        int penaltyModerateRatio,
        int penaltyLimitedRatio,
        int penaltyCriticalRatio,
        decimal rateGood,
        decimal rateModerate,
        decimal rateHeavy,
        int penaltyModerateRate,
        int penaltyHeavyRate,
        int penaltyExcessiveRate,
        int penaltySelfEmployed,
        int penaltyRetired,
        int penaltyUnemployed,
        int penaltyPaymentDefault)
    {
        GreenThreshold = greenThreshold;
        YellowThreshold = yellowThreshold;
        IncomeRatioGood = incomeRatioGood;
        IncomeRatioModerate = incomeRatioModerate;
        IncomeRatioLimited = incomeRatioLimited;
        PenaltyModerateRatio = penaltyModerateRatio;
        PenaltyLimitedRatio = penaltyLimitedRatio;
        PenaltyCriticalRatio = penaltyCriticalRatio;
        RateGood = rateGood;
        RateModerate = rateModerate;
        RateHeavy = rateHeavy;
        PenaltyModerateRate = penaltyModerateRate;
        PenaltyHeavyRate = penaltyHeavyRate;
        PenaltyExcessiveRate = penaltyExcessiveRate;
        PenaltySelfEmployed = penaltySelfEmployed;
        PenaltyRetired = penaltyRetired;
        PenaltyUnemployed = penaltyUnemployed;
        PenaltyPaymentDefault = penaltyPaymentDefault;
    }

    public static ScoringConfig Create(
        int greenThreshold,
        int yellowThreshold,
        decimal incomeRatioGood,
        decimal incomeRatioModerate,
        decimal incomeRatioLimited,
        int penaltyModerateRatio,
        int penaltyLimitedRatio,
        int penaltyCriticalRatio,
        decimal rateGood,
        decimal rateModerate,
        decimal rateHeavy,
        int penaltyModerateRate,
        int penaltyHeavyRate,
        int penaltyExcessiveRate,
        int penaltySelfEmployed,
        int penaltyRetired,
        int penaltyUnemployed,
        int penaltyPaymentDefault)
    {
        ValidateThreshold(greenThreshold, nameof(greenThreshold));
        ValidateThreshold(yellowThreshold, nameof(yellowThreshold));
        if (greenThreshold <= yellowThreshold)
            throw new DomainException("greenThreshold muss größer als yellowThreshold sein");

        ValidateRatio(incomeRatioGood, nameof(incomeRatioGood));
        ValidateRatio(incomeRatioModerate, nameof(incomeRatioModerate));
        ValidateRatio(incomeRatioLimited, nameof(incomeRatioLimited));
        if (incomeRatioGood <= incomeRatioModerate)
            throw new DomainException("incomeRatioGood muss größer als incomeRatioModerate sein");
        if (incomeRatioModerate <= incomeRatioLimited)
            throw new DomainException("incomeRatioModerate muss größer als incomeRatioLimited sein");

        ValidateRatio(rateGood, nameof(rateGood));
        ValidateRatio(rateModerate, nameof(rateModerate));
        ValidateRatio(rateHeavy, nameof(rateHeavy));
        if (rateGood >= rateModerate)
            throw new DomainException("rateGood muss kleiner als rateModerate sein");
        if (rateModerate >= rateHeavy)
            throw new DomainException("rateModerate muss kleiner als rateHeavy sein");

        ValidatePenalty(penaltyModerateRatio, nameof(penaltyModerateRatio));
        ValidatePenalty(penaltyLimitedRatio, nameof(penaltyLimitedRatio));
        ValidatePenalty(penaltyCriticalRatio, nameof(penaltyCriticalRatio));
        ValidatePenalty(penaltyModerateRate, nameof(penaltyModerateRate));
        ValidatePenalty(penaltyHeavyRate, nameof(penaltyHeavyRate));
        ValidatePenalty(penaltyExcessiveRate, nameof(penaltyExcessiveRate));
        ValidatePenalty(penaltySelfEmployed, nameof(penaltySelfEmployed));
        ValidatePenalty(penaltyRetired, nameof(penaltyRetired));
        ValidatePenalty(penaltyUnemployed, nameof(penaltyUnemployed));
        ValidatePenalty(penaltyPaymentDefault, nameof(penaltyPaymentDefault));

        return new ScoringConfig(
            greenThreshold, yellowThreshold,
            incomeRatioGood, incomeRatioModerate, incomeRatioLimited,
            penaltyModerateRatio, penaltyLimitedRatio, penaltyCriticalRatio,
            rateGood, rateModerate, rateHeavy,
            penaltyModerateRate, penaltyHeavyRate, penaltyExcessiveRate,
            penaltySelfEmployed, penaltyRetired, penaltyUnemployed, penaltyPaymentDefault);
    }

    public static ScoringConfig Default => Create(
        75,
        50,
        0.5m,
        0.3m,
        0.1m,
        15,
        30,
        50,
        0.3m,
        0.5m,
        0.7m,
        10,
        25,
        40,
        10,
        5,
        35,
        25);

    private static void ValidateThreshold(int value, string name)
    {
        if (value < 1 || value > 99)
            throw new DomainException($"{name} muss zwischen 1 und 99 liegen");
    }

    private static void ValidateRatio(decimal value, string name)
    {
        if (value < 0.01m || value > 0.99m)
            throw new DomainException($"{name} muss zwischen 0.01 und 0.99 liegen");
    }

    private static void ValidatePenalty(int value, string name)
    {
        if (value < 0 || value > 100)
            throw new DomainException($"{name} muss zwischen 0 und 100 liegen");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return GreenThreshold;
        yield return YellowThreshold;
        yield return IncomeRatioGood;
        yield return IncomeRatioModerate;
        yield return IncomeRatioLimited;
        yield return PenaltyModerateRatio;
        yield return PenaltyLimitedRatio;
        yield return PenaltyCriticalRatio;
        yield return RateGood;
        yield return RateModerate;
        yield return RateHeavy;
        yield return PenaltyModerateRate;
        yield return PenaltyHeavyRate;
        yield return PenaltyExcessiveRate;
        yield return PenaltySelfEmployed;
        yield return PenaltyRetired;
        yield return PenaltyUnemployed;
        yield return PenaltyPaymentDefault;
    }
}