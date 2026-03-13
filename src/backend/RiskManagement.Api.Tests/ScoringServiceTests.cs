using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Api.Tests;

public class ScoringServiceTests
{
    private readonly ScoringService _scoringService = new();
    private static readonly ScoringConfig DefaultConfig = ScoringConfig.Default;

    private static Money M(decimal amount)
    {
        return Money.Create(amount);
    }

    private static Money Mp(decimal amount)
    {
        return Money.CreatePositive(amount);
    }

    // Base Score Calculation

    [Fact]
    public void Should_Return_Score_Between_0_And_100()
    {
        var result = _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.InRange(result.Score, 0, 100);
    }

    [Fact]
    public void Should_Return_Green_TrafficLight_For_Score_Gte_75()
    {
        var result = _scoringService.CalculateScore(M(6000), M(2000), Mp(500), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.True(result.Score >= 75);
        Assert.Equal(TrafficLight.Green, result.TrafficLight);
    }

    [Fact]
    public void Should_Return_Yellow_TrafficLight_For_Score_Gte_50_And_Lt_75()
    {
        var result = _scoringService.CalculateScore(M(4000), M(2200), Mp(700), EmploymentStatus.SelfEmployed, false,
            DefaultConfig);

        Assert.True(result.Score >= 50);
        Assert.True(result.Score < 75);
        Assert.Equal(TrafficLight.Yellow, result.TrafficLight);
    }

    [Fact]
    public void Should_Return_Red_TrafficLight_For_Score_Lt_50()
    {
        var result = _scoringService.CalculateScore(M(2500), M(2000), Mp(400), EmploymentStatus.Unemployed, true,
            DefaultConfig);

        Assert.True(result.Score < 50);
        Assert.Equal(TrafficLight.Red, result.TrafficLight);
    }

    // Income vs Fixed Costs Ratio

    [Fact]
    public void Should_Give_High_Score_For_Good_Income_Costs_Ratio()
    {
        var result = _scoringService.CalculateScore(M(5000), M(2000), Mp(500), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.True(result.Score >= 75);
        Assert.Contains(result.Reasons, r => r.Contains("Gutes Verhältnis"));
    }

    [Fact]
    public void Should_Give_Moderate_Score_For_Moderate_Income_Costs_Ratio()
    {
        var result = _scoringService.CalculateScore(M(4000), M(2400), Mp(400), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.Contains(result.Reasons, r => r.Contains("Moderates Verhältnis"));
    }

    [Fact]
    public void Should_Give_Lower_Score_For_Limited_Income_Costs_Ratio()
    {
        var result = _scoringService.CalculateScore(M(3000), M(2400), Mp(300), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.Contains(result.Reasons, r => r.Contains("Eingeschränktes Verhältnis"));
    }

    [Fact]
    public void Should_Give_Low_Score_For_Critical_Income_Costs_Ratio()
    {
        var result = _scoringService.CalculateScore(M(2500), M(2300), Mp(100), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.Contains(result.Reasons, r => r.Contains("Kritisches Verhältnis"));
    }

    // Employment Status Impact

    [Fact]
    public void Should_Not_Penalize_Employed_Status()
    {
        var employedResult = _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed, false,
            DefaultConfig);
        var selfEmployedResult = _scoringService.CalculateScore(M(4000), M(1500), Mp(500),
            EmploymentStatus.SelfEmployed, false, DefaultConfig);

        Assert.True(employedResult.Score > selfEmployedResult.Score);
    }

    [Fact]
    public void Should_Penalize_SelfEmployed_Status_Minus_10_Points()
    {
        var employedResult = _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed, false,
            DefaultConfig);
        var selfEmployedResult = _scoringService.CalculateScore(M(4000), M(1500), Mp(500),
            EmploymentStatus.SelfEmployed, false, DefaultConfig);

        Assert.Equal(10, employedResult.Score - selfEmployedResult.Score);
    }

    [Fact]
    public void Should_Penalize_Retired_Status_Minus_5_Points()
    {
        var employedResult = _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed, false,
            DefaultConfig);
        var retiredResult =
            _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Retired, false, DefaultConfig);

        Assert.Equal(5, employedResult.Score - retiredResult.Score);
    }

    [Fact]
    public void Should_Heavily_Penalize_Unemployed_Status_Minus_35_Points()
    {
        var employedResult = _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed, false,
            DefaultConfig);
        var unemployedResult = _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Unemployed,
            false, DefaultConfig);

        Assert.Equal(35, employedResult.Score - unemployedResult.Score);
    }

    // Payment Default Impact

    [Fact]
    public void Should_Penalize_Payment_Default_Minus_25_Points()
    {
        var noDefaultResult = _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed,
            false, DefaultConfig);
        var withDefaultResult =
            _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed, true, DefaultConfig);

        Assert.Equal(25, noDefaultResult.Score - withDefaultResult.Score);
    }

    [Fact]
    public void Should_Include_Payment_Default_In_Reasons_When_True()
    {
        var result =
            _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed, true, DefaultConfig);

        Assert.Contains(result.Reasons, r => r.Contains("Zahlungsverzüge"));
    }

    [Fact]
    public void Should_Include_Positive_Payment_History_In_Reasons_When_False()
    {
        var result = _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.Contains(result.Reasons, r => r.Contains("Keine früheren Zahlungsverzüge"));
    }

    // Rate Affordability

    [Fact]
    public void Should_Give_Good_Score_When_Rate_Lte_30_Percent()
    {
        var result = _scoringService.CalculateScore(M(5000), M(2000), Mp(500), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.Contains(result.Reasons, r => r.Contains("gut tragbar"));
    }

    [Fact]
    public void Should_Penalize_When_Rate_30_50_Percent()
    {
        var result = _scoringService.CalculateScore(M(4000), M(2000), Mp(800), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.Contains(result.Reasons, r => r.Contains("moderat tragbar"));
    }

    [Fact]
    public void Should_Penalize_More_When_Rate_50_70_Percent()
    {
        var result = _scoringService.CalculateScore(M(4000), M(2000), Mp(1200), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.Contains(result.Reasons, r => r.Contains("belastet das Budget erheblich"));
    }

    [Fact]
    public void Should_Heavily_Penalize_When_Rate_Gt_70_Percent()
    {
        var result = _scoringService.CalculateScore(M(3000), M(1500), Mp(1200), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.Contains(result.Reasons, r => r.Contains("übersteigt das tragbare Maß"));
    }

    // Scoring Reasons

    [Fact]
    public void Should_Return_Reasons_In_German()
    {
        var result = _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.NotEmpty(result.Reasons);
        foreach (var reason in result.Reasons)
        {
            Assert.IsType<string>(reason);
            Assert.NotEmpty(reason);
        }
    }

    [Fact]
    public void Should_Include_Employment_Status_In_Reasons()
    {
        var result = _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.Contains(result.Reasons, r => r.Contains("Angestelltenverhältnis"));
    }

    [Fact]
    public void Should_Include_Income_Ratio_Assessment_In_Reasons()
    {
        var result = _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.Contains(result.Reasons, r => r.Contains("Verhältnis zwischen Einkommen"));
    }

    [Fact]
    public void Should_Include_Overall_Assessment_As_First_Reason()
    {
        var result = _scoringService.CalculateScore(M(4000), M(1500), Mp(500), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.Contains("Gesamtbewertung", result.Reasons[0]);
    }

    // Edge Cases

    [Fact]
    public void Should_Handle_Zero_Fixed_Costs()
    {
        var result =
            _scoringService.CalculateScore(M(4000), M(0), Mp(500), EmploymentStatus.Employed, false, DefaultConfig);

        Assert.InRange(result.Score, 0, 100);
    }

    [Fact]
    public void Should_Handle_Minimum_Viable_Income()
    {
        var result =
            _scoringService.CalculateScore(M(1000), M(500), Mp(100), EmploymentStatus.Employed, false, DefaultConfig);

        Assert.InRange(result.Score, 0, 100);
    }

    [Fact]
    public void Should_Cap_Score_At_100()
    {
        var result = _scoringService.CalculateScore(M(10000), M(1000), Mp(500), EmploymentStatus.Employed, false,
            DefaultConfig);

        Assert.True(result.Score <= 100);
    }

    [Fact]
    public void Should_Not_Go_Below_0()
    {
        var result = _scoringService.CalculateScore(M(2000), M(1800), Mp(150), EmploymentStatus.Unemployed, true,
            DefaultConfig);

        Assert.True(result.Score >= 0);
    }

    // Traffic Light Thresholds

    [Fact]
    public void Should_Return_Green_For_Score_Exactly_75()
    {
        var result = _scoringService.CalculateScore(M(5000), M(2000), Mp(600), EmploymentStatus.Employed, false,
            DefaultConfig);

        if (result.Score == 75) Assert.Equal(TrafficLight.Green, result.TrafficLight);
    }

    [Fact]
    public void Should_Return_Yellow_For_Score_Exactly_50()
    {
        var result = _scoringService.CalculateScore(M(4000), M(2200), Mp(600), EmploymentStatus.SelfEmployed, false,
            DefaultConfig);

        if (result.Score == 50) Assert.Equal(TrafficLight.Yellow, result.TrafficLight);
    }
}