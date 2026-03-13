using FluentAssertions;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Exceptions;

namespace RiskManagement.Api.Tests;

public class ScoringConfigTests
{
    [Fact]
    public void Create_WithValidDefaults_ShouldSucceed()
    {
        var config = ScoringConfig.Default;

        config.GreenThreshold.Should().Be(75);
        config.YellowThreshold.Should().Be(50);
        config.IncomeRatioGood.Should().Be(0.5m);
        config.PenaltyPaymentDefault.Should().Be(25);
    }

    [Fact]
    public void Create_WithValidCustomValues_ShouldSucceed()
    {
        var config = ScoringConfig.Create(
            greenThreshold: 80, yellowThreshold: 40,
            incomeRatioGood: 0.6m, incomeRatioModerate: 0.4m, incomeRatioLimited: 0.2m,
            penaltyModerateRatio: 10, penaltyLimitedRatio: 20, penaltyCriticalRatio: 40,
            rateGood: 0.2m, rateModerate: 0.4m, rateHeavy: 0.6m,
            penaltyModerateRate: 5, penaltyHeavyRate: 15, penaltyExcessiveRate: 30,
            penaltySelfEmployed: 8, penaltyRetired: 3, penaltyUnemployed: 30,
            penaltyPaymentDefault: 20);

        config.GreenThreshold.Should().Be(80);
        config.YellowThreshold.Should().Be(40);
        config.PenaltySelfEmployed.Should().Be(8);
    }

    [Fact]
    public void Create_GreenThresholdNotGreaterThanYellow_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            greenThreshold: 50, yellowThreshold: 50,
            incomeRatioGood: 0.5m, incomeRatioModerate: 0.3m, incomeRatioLimited: 0.1m,
            penaltyModerateRatio: 15, penaltyLimitedRatio: 30, penaltyCriticalRatio: 50,
            rateGood: 0.3m, rateModerate: 0.5m, rateHeavy: 0.7m,
            penaltyModerateRate: 10, penaltyHeavyRate: 25, penaltyExcessiveRate: 40,
            penaltySelfEmployed: 10, penaltyRetired: 5, penaltyUnemployed: 35,
            penaltyPaymentDefault: 25);

        act.Should().Throw<DomainException>().WithMessage("*greenThreshold*größer*yellowThreshold*");
    }

    [Fact]
    public void Create_GreenThresholdOutOfRange_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            greenThreshold: 0, yellowThreshold: 50,
            incomeRatioGood: 0.5m, incomeRatioModerate: 0.3m, incomeRatioLimited: 0.1m,
            penaltyModerateRatio: 15, penaltyLimitedRatio: 30, penaltyCriticalRatio: 50,
            rateGood: 0.3m, rateModerate: 0.5m, rateHeavy: 0.7m,
            penaltyModerateRate: 10, penaltyHeavyRate: 25, penaltyExcessiveRate: 40,
            penaltySelfEmployed: 10, penaltyRetired: 5, penaltyUnemployed: 35,
            penaltyPaymentDefault: 25);

        act.Should().Throw<DomainException>().WithMessage("*zwischen 1 und 99*");
    }

    [Fact]
    public void Create_IncomeRatioGoodNotGreaterThanModerate_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            greenThreshold: 75, yellowThreshold: 50,
            incomeRatioGood: 0.3m, incomeRatioModerate: 0.3m, incomeRatioLimited: 0.1m,
            penaltyModerateRatio: 15, penaltyLimitedRatio: 30, penaltyCriticalRatio: 50,
            rateGood: 0.3m, rateModerate: 0.5m, rateHeavy: 0.7m,
            penaltyModerateRate: 10, penaltyHeavyRate: 25, penaltyExcessiveRate: 40,
            penaltySelfEmployed: 10, penaltyRetired: 5, penaltyUnemployed: 35,
            penaltyPaymentDefault: 25);

        act.Should().Throw<DomainException>().WithMessage("*incomeRatioGood*größer*incomeRatioModerate*");
    }

    [Fact]
    public void Create_IncomeRatioModerateNotGreaterThanLimited_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            greenThreshold: 75, yellowThreshold: 50,
            incomeRatioGood: 0.5m, incomeRatioModerate: 0.1m, incomeRatioLimited: 0.1m,
            penaltyModerateRatio: 15, penaltyLimitedRatio: 30, penaltyCriticalRatio: 50,
            rateGood: 0.3m, rateModerate: 0.5m, rateHeavy: 0.7m,
            penaltyModerateRate: 10, penaltyHeavyRate: 25, penaltyExcessiveRate: 40,
            penaltySelfEmployed: 10, penaltyRetired: 5, penaltyUnemployed: 35,
            penaltyPaymentDefault: 25);

        act.Should().Throw<DomainException>().WithMessage("*incomeRatioModerate*größer*incomeRatioLimited*");
    }

    [Fact]
    public void Create_RateGoodNotLessThanModerate_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            greenThreshold: 75, yellowThreshold: 50,
            incomeRatioGood: 0.5m, incomeRatioModerate: 0.3m, incomeRatioLimited: 0.1m,
            penaltyModerateRatio: 15, penaltyLimitedRatio: 30, penaltyCriticalRatio: 50,
            rateGood: 0.5m, rateModerate: 0.5m, rateHeavy: 0.7m,
            penaltyModerateRate: 10, penaltyHeavyRate: 25, penaltyExcessiveRate: 40,
            penaltySelfEmployed: 10, penaltyRetired: 5, penaltyUnemployed: 35,
            penaltyPaymentDefault: 25);

        act.Should().Throw<DomainException>().WithMessage("*rateGood*kleiner*rateModerate*");
    }

    [Fact]
    public void Create_RateModerateNotLessThanHeavy_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            greenThreshold: 75, yellowThreshold: 50,
            incomeRatioGood: 0.5m, incomeRatioModerate: 0.3m, incomeRatioLimited: 0.1m,
            penaltyModerateRatio: 15, penaltyLimitedRatio: 30, penaltyCriticalRatio: 50,
            rateGood: 0.3m, rateModerate: 0.7m, rateHeavy: 0.7m,
            penaltyModerateRate: 10, penaltyHeavyRate: 25, penaltyExcessiveRate: 40,
            penaltySelfEmployed: 10, penaltyRetired: 5, penaltyUnemployed: 35,
            penaltyPaymentDefault: 25);

        act.Should().Throw<DomainException>().WithMessage("*rateModerate*kleiner*rateHeavy*");
    }

    [Fact]
    public void Create_NegativePenalty_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            greenThreshold: 75, yellowThreshold: 50,
            incomeRatioGood: 0.5m, incomeRatioModerate: 0.3m, incomeRatioLimited: 0.1m,
            penaltyModerateRatio: -1, penaltyLimitedRatio: 30, penaltyCriticalRatio: 50,
            rateGood: 0.3m, rateModerate: 0.5m, rateHeavy: 0.7m,
            penaltyModerateRate: 10, penaltyHeavyRate: 25, penaltyExcessiveRate: 40,
            penaltySelfEmployed: 10, penaltyRetired: 5, penaltyUnemployed: 35,
            penaltyPaymentDefault: 25);

        act.Should().Throw<DomainException>().WithMessage("*zwischen 0 und 100*");
    }

    [Fact]
    public void Create_PenaltyOver100_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            greenThreshold: 75, yellowThreshold: 50,
            incomeRatioGood: 0.5m, incomeRatioModerate: 0.3m, incomeRatioLimited: 0.1m,
            penaltyModerateRatio: 15, penaltyLimitedRatio: 30, penaltyCriticalRatio: 50,
            rateGood: 0.3m, rateModerate: 0.5m, rateHeavy: 0.7m,
            penaltyModerateRate: 10, penaltyHeavyRate: 25, penaltyExcessiveRate: 40,
            penaltySelfEmployed: 10, penaltyRetired: 5, penaltyUnemployed: 35,
            penaltyPaymentDefault: 101);

        act.Should().Throw<DomainException>().WithMessage("*zwischen 0 und 100*");
    }

    [Fact]
    public void Create_RatioOutOfRange_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            greenThreshold: 75, yellowThreshold: 50,
            incomeRatioGood: 1.5m, incomeRatioModerate: 0.3m, incomeRatioLimited: 0.1m,
            penaltyModerateRatio: 15, penaltyLimitedRatio: 30, penaltyCriticalRatio: 50,
            rateGood: 0.3m, rateModerate: 0.5m, rateHeavy: 0.7m,
            penaltyModerateRate: 10, penaltyHeavyRate: 25, penaltyExcessiveRate: 40,
            penaltySelfEmployed: 10, penaltyRetired: 5, penaltyUnemployed: 35,
            penaltyPaymentDefault: 25);

        act.Should().Throw<DomainException>().WithMessage("*zwischen 0.01 und 0.99*");
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var config1 = ScoringConfig.Default;
        var config2 = ScoringConfig.Default;

        config1.Should().Be(config2);
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        var config1 = ScoringConfig.Default;
        var config2 = ScoringConfig.Create(
            greenThreshold: 80, yellowThreshold: 50,
            incomeRatioGood: 0.5m, incomeRatioModerate: 0.3m, incomeRatioLimited: 0.1m,
            penaltyModerateRatio: 15, penaltyLimitedRatio: 30, penaltyCriticalRatio: 50,
            rateGood: 0.3m, rateModerate: 0.5m, rateHeavy: 0.7m,
            penaltyModerateRate: 10, penaltyHeavyRate: 25, penaltyExcessiveRate: 40,
            penaltySelfEmployed: 10, penaltyRetired: 5, penaltyUnemployed: 35,
            penaltyPaymentDefault: 25);

        config1.Should().NotBe(config2);
    }
}
