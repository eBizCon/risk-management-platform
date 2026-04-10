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

    public int CreditScoreGood { get; }
    public int CreditScoreModerate { get; }
    public int PenaltyModerateCreditScore { get; }
    public int PenaltyLowCreditScore { get; }

    public decimal LoanToIncomeRatioGood { get; }
    public decimal LoanToIncomeRatioModerate { get; }
    public decimal LoanToIncomeRatioHigh { get; }
    public int PenaltyModerateLoanToIncome { get; }
    public int PenaltyHighLoanToIncome { get; }
    public int PenaltyCriticalLoanToIncome { get; }

    public int LoanTermShort { get; }
    public int LoanTermMedium { get; }
    public int LoanTermLong { get; }
    public int PenaltyMediumLoanTerm { get; }
    public int PenaltyLongLoanTerm { get; }
    public int PenaltyVeryLongLoanTerm { get; }

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
        int penaltyPaymentDefault,
        int creditScoreGood,
        int creditScoreModerate,
        int penaltyModerateCreditScore,
        int penaltyLowCreditScore,
        decimal loanToIncomeRatioGood = 2.0m,
        decimal loanToIncomeRatioModerate = 4.0m,
        decimal loanToIncomeRatioHigh = 6.0m,
        int penaltyModerateLoanToIncome = 10,
        int penaltyHighLoanToIncome = 20,
        int penaltyCriticalLoanToIncome = 35,
        int loanTermShort = 24,
        int loanTermMedium = 60,
        int loanTermLong = 120,
        int penaltyMediumLoanTerm = 5,
        int penaltyLongLoanTerm = 15,
        int penaltyVeryLongLoanTerm = 25)
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
        CreditScoreGood = creditScoreGood;
        CreditScoreModerate = creditScoreModerate;
        PenaltyModerateCreditScore = penaltyModerateCreditScore;
        PenaltyLowCreditScore = penaltyLowCreditScore;
        LoanToIncomeRatioGood = loanToIncomeRatioGood;
        LoanToIncomeRatioModerate = loanToIncomeRatioModerate;
        LoanToIncomeRatioHigh = loanToIncomeRatioHigh;
        PenaltyModerateLoanToIncome = penaltyModerateLoanToIncome;
        PenaltyHighLoanToIncome = penaltyHighLoanToIncome;
        PenaltyCriticalLoanToIncome = penaltyCriticalLoanToIncome;
        LoanTermShort = loanTermShort;
        LoanTermMedium = loanTermMedium;
        LoanTermLong = loanTermLong;
        PenaltyMediumLoanTerm = penaltyMediumLoanTerm;
        PenaltyLongLoanTerm = penaltyLongLoanTerm;
        PenaltyVeryLongLoanTerm = penaltyVeryLongLoanTerm;
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
        int penaltyPaymentDefault,
        int creditScoreGood,
        int creditScoreModerate,
        int penaltyModerateCreditScore,
        int penaltyLowCreditScore,
        decimal loanToIncomeRatioGood = 2.0m,
        decimal loanToIncomeRatioModerate = 4.0m,
        decimal loanToIncomeRatioHigh = 6.0m,
        int penaltyModerateLoanToIncome = 10,
        int penaltyHighLoanToIncome = 20,
        int penaltyCriticalLoanToIncome = 35,
        int loanTermShort = 24,
        int loanTermMedium = 60,
        int loanTermLong = 120,
        int penaltyMediumLoanTerm = 5,
        int penaltyLongLoanTerm = 15,
        int penaltyVeryLongLoanTerm = 25)
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

        ValidateCreditScore(creditScoreGood, nameof(creditScoreGood));
        ValidateCreditScore(creditScoreModerate, nameof(creditScoreModerate));
        if (creditScoreGood <= creditScoreModerate)
            throw new DomainException("creditScoreGood muss größer als creditScoreModerate sein");
        ValidatePenalty(penaltyModerateCreditScore, nameof(penaltyModerateCreditScore));
        ValidatePenalty(penaltyLowCreditScore, nameof(penaltyLowCreditScore));

        ValidateLoanToIncomeRatio(loanToIncomeRatioGood, nameof(loanToIncomeRatioGood));
        ValidateLoanToIncomeRatio(loanToIncomeRatioModerate, nameof(loanToIncomeRatioModerate));
        ValidateLoanToIncomeRatio(loanToIncomeRatioHigh, nameof(loanToIncomeRatioHigh));
        if (loanToIncomeRatioGood >= loanToIncomeRatioModerate)
            throw new DomainException("loanToIncomeRatioGood muss kleiner als loanToIncomeRatioModerate sein");
        if (loanToIncomeRatioModerate >= loanToIncomeRatioHigh)
            throw new DomainException("loanToIncomeRatioModerate muss kleiner als loanToIncomeRatioHigh sein");
        ValidatePenalty(penaltyModerateLoanToIncome, nameof(penaltyModerateLoanToIncome));
        ValidatePenalty(penaltyHighLoanToIncome, nameof(penaltyHighLoanToIncome));
        ValidatePenalty(penaltyCriticalLoanToIncome, nameof(penaltyCriticalLoanToIncome));

        ValidateLoanTerm(loanTermShort, nameof(loanTermShort));
        ValidateLoanTerm(loanTermMedium, nameof(loanTermMedium));
        ValidateLoanTerm(loanTermLong, nameof(loanTermLong));
        if (loanTermShort >= loanTermMedium)
            throw new DomainException("loanTermShort muss kleiner als loanTermMedium sein");
        if (loanTermMedium >= loanTermLong)
            throw new DomainException("loanTermMedium muss kleiner als loanTermLong sein");
        ValidatePenalty(penaltyMediumLoanTerm, nameof(penaltyMediumLoanTerm));
        ValidatePenalty(penaltyLongLoanTerm, nameof(penaltyLongLoanTerm));
        ValidatePenalty(penaltyVeryLongLoanTerm, nameof(penaltyVeryLongLoanTerm));

        return new ScoringConfig(
            greenThreshold, yellowThreshold,
            incomeRatioGood, incomeRatioModerate, incomeRatioLimited,
            penaltyModerateRatio, penaltyLimitedRatio, penaltyCriticalRatio,
            rateGood, rateModerate, rateHeavy,
            penaltyModerateRate, penaltyHeavyRate, penaltyExcessiveRate,
            penaltySelfEmployed, penaltyRetired, penaltyUnemployed, penaltyPaymentDefault,
            creditScoreGood, creditScoreModerate, penaltyModerateCreditScore, penaltyLowCreditScore,
            loanToIncomeRatioGood, loanToIncomeRatioModerate, loanToIncomeRatioHigh,
            penaltyModerateLoanToIncome, penaltyHighLoanToIncome, penaltyCriticalLoanToIncome,
            loanTermShort, loanTermMedium, loanTermLong,
            penaltyMediumLoanTerm, penaltyLongLoanTerm, penaltyVeryLongLoanTerm);
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
        25,
        400,
        250,
        10,
        20);

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

    private static void ValidateCreditScore(int value, string name)
    {
        if (value < 100 || value > 600)
            throw new DomainException($"{name} muss zwischen 100 und 600 liegen");
    }

    private static void ValidateLoanToIncomeRatio(decimal value, string name)
    {
        if (value <= 0 || value > 100)
            throw new DomainException($"{name} muss zwischen 0 und 100 liegen");
    }

    private static void ValidateLoanTerm(int value, string name)
    {
        if (value <= 0 || value > 360)
            throw new DomainException($"{name} muss zwischen 1 und 360 liegen");
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
        yield return CreditScoreGood;
        yield return CreditScoreModerate;
        yield return PenaltyModerateCreditScore;
        yield return PenaltyLowCreditScore;
        yield return LoanToIncomeRatioGood;
        yield return LoanToIncomeRatioModerate;
        yield return LoanToIncomeRatioHigh;
        yield return PenaltyModerateLoanToIncome;
        yield return PenaltyHighLoanToIncome;
        yield return PenaltyCriticalLoanToIncome;
        yield return LoanTermShort;
        yield return LoanTermMedium;
        yield return LoanTermLong;
        yield return PenaltyMediumLoanTerm;
        yield return PenaltyLongLoanTerm;
        yield return PenaltyVeryLongLoanTerm;
    }
}
