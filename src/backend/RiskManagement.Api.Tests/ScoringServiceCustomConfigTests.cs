using FluentAssertions;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;

namespace RiskManagement.Api.Tests;

public class ScoringServiceCustomConfigTests
{
    private readonly ScoringService _scoringService = new();

    private static Money M(decimal amount)
    {
        return Money.Create(amount);
    }

    private static Money Mp(decimal amount)
    {
        return Money.CreatePositive(amount);
    }

    private static ScoringConfig CreateCustomConfig(
        int greenThreshold = 75, int yellowThreshold = 50,
        decimal incomeRatioGood = 0.5m, decimal incomeRatioModerate = 0.3m, decimal incomeRatioLimited = 0.1m,
        int penaltyModerateRatio = 15, int penaltyLimitedRatio = 30, int penaltyCriticalRatio = 50,
        decimal rateGood = 0.3m, decimal rateModerate = 0.5m, decimal rateHeavy = 0.7m,
        int penaltyModerateRate = 10, int penaltyHeavyRate = 25, int penaltyExcessiveRate = 40,
        int penaltySelfEmployed = 10, int penaltyRetired = 5, int penaltyUnemployed = 35,
        int penaltyPaymentDefault = 25,
        int creditScoreGood = 400, int creditScoreModerate = 250,
        int penaltyModerateCreditScore = 10, int penaltyLowCreditScore = 20)
    {
        return ScoringConfig.Create(
            greenThreshold, yellowThreshold,
            incomeRatioGood, incomeRatioModerate, incomeRatioLimited,
            penaltyModerateRatio, penaltyLimitedRatio, penaltyCriticalRatio,
            rateGood, rateModerate, rateHeavy,
            penaltyModerateRate, penaltyHeavyRate, penaltyExcessiveRate,
            penaltySelfEmployed, penaltyRetired, penaltyUnemployed, penaltyPaymentDefault,
            creditScoreGood, creditScoreModerate, penaltyModerateCreditScore, penaltyLowCreditScore);
    }

    [Fact]
    public void CalculateScore_HigherPaymentDefaultPenalty_ShouldReduceScoreMore()
    {
        var defaultConfig = ScoringConfig.Default;
        var strictConfig = CreateCustomConfig(penaltyPaymentDefault: 50);

        var defaultResult =
            _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed, true, null, defaultConfig);
        var strictResult =
            _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed, true, null, strictConfig);

        strictResult.Score.Should().BeLessThan(defaultResult.Score);
    }

    [Fact]
    public void CalculateScore_HigherUnemployedPenalty_ShouldReduceScoreMore()
    {
        var defaultConfig = ScoringConfig.Default;
        var strictConfig = CreateCustomConfig(penaltyUnemployed: 50);

        var defaultResult = _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Unemployed,
            false, null, defaultConfig);
        var strictResult = _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Unemployed, false,
            null, strictConfig);

        strictResult.Score.Should().BeLessThan(defaultResult.Score);
    }

    [Fact]
    public void CalculateScore_LowerGreenThreshold_ShouldChangeTrafficLight()
    {
        var defaultConfig = ScoringConfig.Default;
        var lenientConfig = CreateCustomConfig(60, 30);

        var defaultResult = _scoringService.CalculateScore(M(4000), M(2000), Mp(500), EmploymentStatus.SelfEmployed,
            false, null, defaultConfig);
        var lenientResult = _scoringService.CalculateScore(M(4000), M(2000), Mp(500), EmploymentStatus.SelfEmployed,
            false, null, lenientConfig);

        defaultResult.Score.Should().Be(lenientResult.Score);

        if (defaultResult.Score >= 60 && defaultResult.Score < 75)
        {
            lenientResult.TrafficLight.Should().Be(TrafficLight.Green);
            defaultResult.TrafficLight.Should().NotBe(TrafficLight.Green);
        }
    }

    [Fact]
    public void CalculateScore_ZeroPenalties_ShouldGiveMaxScore()
    {
        var zeroPenaltyConfig = CreateCustomConfig(
            penaltyModerateRatio: 0, penaltyLimitedRatio: 0, penaltyCriticalRatio: 0,
            penaltyModerateRate: 0, penaltyHeavyRate: 0, penaltyExcessiveRate: 0,
            penaltySelfEmployed: 0, penaltyRetired: 0, penaltyUnemployed: 0,
            penaltyPaymentDefault: 0);

        var result = _scoringService.CalculateScore(M(2000), M(1800), Mp(150), EmploymentStatus.Unemployed, true,
            null, zeroPenaltyConfig);

        result.Score.Should().Be(100);
    }

    [Fact]
    public void CalculateScore_DifferentSelfEmployedPenalty_ShouldReflectInScore()
    {
        var lowPenalty = CreateCustomConfig(penaltySelfEmployed: 5);
        var highPenalty = CreateCustomConfig(penaltySelfEmployed: 20);

        var lowResult = _scoringService.CalculateScore(M(5000), M(2000), Mp(500), EmploymentStatus.SelfEmployed, false,
            null, lowPenalty);
        var highResult = _scoringService.CalculateScore(M(5000), M(2000), Mp(500), EmploymentStatus.SelfEmployed, false,
            null, highPenalty);

        (lowResult.Score - highResult.Score).Should().Be(15);
    }

    [Fact]
    public void CalculateScore_CustomConfig_ScoreShouldStayInRange()
    {
        var strictConfig = CreateCustomConfig(
            penaltyModerateRatio: 50, penaltyLimitedRatio: 80, penaltyCriticalRatio: 100,
            penaltyModerateRate: 50, penaltyHeavyRate: 80, penaltyExcessiveRate: 100,
            penaltySelfEmployed: 50, penaltyRetired: 30, penaltyUnemployed: 80,
            penaltyPaymentDefault: 80);

        var result = _scoringService.CalculateScore(M(2000), M(1800), Mp(150), EmploymentStatus.Unemployed, true,
            null, strictConfig);

        result.Score.Should().BeInRange(0, 100);
    }
}
