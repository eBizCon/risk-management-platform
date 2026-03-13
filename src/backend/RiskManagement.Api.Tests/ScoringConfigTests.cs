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
            80, 40,
            0.6m, 0.4m, 0.2m,
            10, 20, 40,
            0.2m, 0.4m, 0.6m,
            5, 15, 30,
            8, 3, 30,
            20);

        config.GreenThreshold.Should().Be(80);
        config.YellowThreshold.Should().Be(40);
        config.PenaltySelfEmployed.Should().Be(8);
    }

    [Fact]
    public void Create_GreenThresholdNotGreaterThanYellow_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            50, 50,
            0.5m, 0.3m, 0.1m,
            15, 30, 50,
            0.3m, 0.5m, 0.7m,
            10, 25, 40,
            10, 5, 35,
            25);

        act.Should().Throw<DomainException>().WithMessage("*greenThreshold*größer*yellowThreshold*");
    }

    [Fact]
    public void Create_GreenThresholdOutOfRange_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            0, 50,
            0.5m, 0.3m, 0.1m,
            15, 30, 50,
            0.3m, 0.5m, 0.7m,
            10, 25, 40,
            10, 5, 35,
            25);

        act.Should().Throw<DomainException>().WithMessage("*zwischen 1 und 99*");
    }

    [Fact]
    public void Create_IncomeRatioGoodNotGreaterThanModerate_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            75, 50,
            0.3m, 0.3m, 0.1m,
            15, 30, 50,
            0.3m, 0.5m, 0.7m,
            10, 25, 40,
            10, 5, 35,
            25);

        act.Should().Throw<DomainException>().WithMessage("*incomeRatioGood*größer*incomeRatioModerate*");
    }

    [Fact]
    public void Create_IncomeRatioModerateNotGreaterThanLimited_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            75, 50,
            0.5m, 0.1m, 0.1m,
            15, 30, 50,
            0.3m, 0.5m, 0.7m,
            10, 25, 40,
            10, 5, 35,
            25);

        act.Should().Throw<DomainException>().WithMessage("*incomeRatioModerate*größer*incomeRatioLimited*");
    }

    [Fact]
    public void Create_RateGoodNotLessThanModerate_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            75, 50,
            0.5m, 0.3m, 0.1m,
            15, 30, 50,
            0.5m, 0.5m, 0.7m,
            10, 25, 40,
            10, 5, 35,
            25);

        act.Should().Throw<DomainException>().WithMessage("*rateGood*kleiner*rateModerate*");
    }

    [Fact]
    public void Create_RateModerateNotLessThanHeavy_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            75, 50,
            0.5m, 0.3m, 0.1m,
            15, 30, 50,
            0.3m, 0.7m, 0.7m,
            10, 25, 40,
            10, 5, 35,
            25);

        act.Should().Throw<DomainException>().WithMessage("*rateModerate*kleiner*rateHeavy*");
    }

    [Fact]
    public void Create_NegativePenalty_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            75, 50,
            0.5m, 0.3m, 0.1m,
            -1, 30, 50,
            0.3m, 0.5m, 0.7m,
            10, 25, 40,
            10, 5, 35,
            25);

        act.Should().Throw<DomainException>().WithMessage("*zwischen 0 und 100*");
    }

    [Fact]
    public void Create_PenaltyOver100_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            75, 50,
            0.5m, 0.3m, 0.1m,
            15, 30, 50,
            0.3m, 0.5m, 0.7m,
            10, 25, 40,
            10, 5, 35,
            101);

        act.Should().Throw<DomainException>().WithMessage("*zwischen 0 und 100*");
    }

    [Fact]
    public void Create_RatioOutOfRange_ShouldThrow()
    {
        var act = () => ScoringConfig.Create(
            75, 50,
            1.5m, 0.3m, 0.1m,
            15, 30, 50,
            0.3m, 0.5m, 0.7m,
            10, 25, 40,
            10, 5, 35,
            25);

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
            80, 50,
            0.5m, 0.3m, 0.1m,
            15, 30, 50,
            0.3m, 0.5m, 0.7m,
            10, 25, 40,
            10, 5, 35,
            25);

        config1.Should().NotBe(config2);
    }
}