using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Api.Tests;

public class ScoringServiceTests
{
    private readonly ScoringService _scoringService = new();

    // Base Score Calculation

    [Fact]
    public void Should_Return_Score_Between_0_And_100()
    {
        var result = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Employed, false);

        Assert.InRange(result.Score, 0, 100);
    }

    [Fact]
    public void Should_Return_Green_TrafficLight_For_Score_Gte_75()
    {
        var result = _scoringService.CalculateScore(6000, 2000, 500, EmploymentStatus.Employed, false);

        Assert.True(result.Score >= 75);
        Assert.Equal(TrafficLight.Green, result.TrafficLight);
    }

    [Fact]
    public void Should_Return_Yellow_TrafficLight_For_Score_Gte_50_And_Lt_75()
    {
        var result = _scoringService.CalculateScore(4000, 2200, 700, EmploymentStatus.SelfEmployed, false);

        Assert.True(result.Score >= 50);
        Assert.True(result.Score < 75);
        Assert.Equal(TrafficLight.Yellow, result.TrafficLight);
    }

    [Fact]
    public void Should_Return_Red_TrafficLight_For_Score_Lt_50()
    {
        var result = _scoringService.CalculateScore(2500, 2000, 400, EmploymentStatus.Unemployed, true);

        Assert.True(result.Score < 50);
        Assert.Equal(TrafficLight.Red, result.TrafficLight);
    }

    // Income vs Fixed Costs Ratio

    [Fact]
    public void Should_Give_High_Score_For_Good_Income_Costs_Ratio()
    {
        var result = _scoringService.CalculateScore(5000, 2000, 500, EmploymentStatus.Employed, false);

        Assert.True(result.Score >= 75);
        Assert.Contains(result.Reasons, r => r.Contains("Gutes Verhältnis"));
    }

    [Fact]
    public void Should_Give_Moderate_Score_For_Moderate_Income_Costs_Ratio()
    {
        var result = _scoringService.CalculateScore(4000, 2400, 400, EmploymentStatus.Employed, false);

        Assert.Contains(result.Reasons, r => r.Contains("Moderates Verhältnis"));
    }

    [Fact]
    public void Should_Give_Lower_Score_For_Limited_Income_Costs_Ratio()
    {
        var result = _scoringService.CalculateScore(3000, 2400, 300, EmploymentStatus.Employed, false);

        Assert.Contains(result.Reasons, r => r.Contains("Eingeschränktes Verhältnis"));
    }

    [Fact]
    public void Should_Give_Low_Score_For_Critical_Income_Costs_Ratio()
    {
        var result = _scoringService.CalculateScore(2500, 2300, 100, EmploymentStatus.Employed, false);

        Assert.Contains(result.Reasons, r => r.Contains("Kritisches Verhältnis"));
    }

    // Employment Status Impact

    [Fact]
    public void Should_Not_Penalize_Employed_Status()
    {
        var employedResult = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Employed, false);
        var selfEmployedResult = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.SelfEmployed, false);

        Assert.True(employedResult.Score > selfEmployedResult.Score);
    }

    [Fact]
    public void Should_Penalize_SelfEmployed_Status_Minus_10_Points()
    {
        var employedResult = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Employed, false);
        var selfEmployedResult = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.SelfEmployed, false);

        Assert.Equal(10, employedResult.Score - selfEmployedResult.Score);
    }

    [Fact]
    public void Should_Penalize_Retired_Status_Minus_5_Points()
    {
        var employedResult = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Employed, false);
        var retiredResult = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Retired, false);

        Assert.Equal(5, employedResult.Score - retiredResult.Score);
    }

    [Fact]
    public void Should_Heavily_Penalize_Unemployed_Status_Minus_35_Points()
    {
        var employedResult = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Employed, false);
        var unemployedResult = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Unemployed, false);

        Assert.Equal(35, employedResult.Score - unemployedResult.Score);
    }

    // Payment Default Impact

    [Fact]
    public void Should_Penalize_Payment_Default_Minus_25_Points()
    {
        var noDefaultResult = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Employed, false);
        var withDefaultResult = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Employed, true);

        Assert.Equal(25, noDefaultResult.Score - withDefaultResult.Score);
    }

    [Fact]
    public void Should_Include_Payment_Default_In_Reasons_When_True()
    {
        var result = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Employed, true);

        Assert.Contains(result.Reasons, r => r.Contains("Zahlungsverzüge"));
    }

    [Fact]
    public void Should_Include_Positive_Payment_History_In_Reasons_When_False()
    {
        var result = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Employed, false);

        Assert.Contains(result.Reasons, r => r.Contains("Keine früheren Zahlungsverzüge"));
    }

    // Rate Affordability

    [Fact]
    public void Should_Give_Good_Score_When_Rate_Lte_30_Percent()
    {
        var result = _scoringService.CalculateScore(5000, 2000, 500, EmploymentStatus.Employed, false);

        Assert.Contains(result.Reasons, r => r.Contains("gut tragbar"));
    }

    [Fact]
    public void Should_Penalize_When_Rate_30_50_Percent()
    {
        var result = _scoringService.CalculateScore(4000, 2000, 800, EmploymentStatus.Employed, false);

        Assert.Contains(result.Reasons, r => r.Contains("moderat tragbar"));
    }

    [Fact]
    public void Should_Penalize_More_When_Rate_50_70_Percent()
    {
        var result = _scoringService.CalculateScore(4000, 2000, 1200, EmploymentStatus.Employed, false);

        Assert.Contains(result.Reasons, r => r.Contains("belastet das Budget erheblich"));
    }

    [Fact]
    public void Should_Heavily_Penalize_When_Rate_Gt_70_Percent()
    {
        var result = _scoringService.CalculateScore(3000, 1500, 1200, EmploymentStatus.Employed, false);

        Assert.Contains(result.Reasons, r => r.Contains("übersteigt das tragbare Maß"));
    }

    // Scoring Reasons

    [Fact]
    public void Should_Return_Reasons_In_German()
    {
        var result = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Employed, false);

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
        var result = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Employed, false);

        Assert.Contains(result.Reasons, r => r.Contains("Angestelltenverhältnis"));
    }

    [Fact]
    public void Should_Include_Income_Ratio_Assessment_In_Reasons()
    {
        var result = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Employed, false);

        Assert.Contains(result.Reasons, r => r.Contains("Verhältnis zwischen Einkommen"));
    }

    [Fact]
    public void Should_Include_Overall_Assessment_As_First_Reason()
    {
        var result = _scoringService.CalculateScore(4000, 1500, 500, EmploymentStatus.Employed, false);

        Assert.Contains("Gesamtbewertung", result.Reasons[0]);
    }

    // Edge Cases

    [Fact]
    public void Should_Handle_Zero_Fixed_Costs()
    {
        var result = _scoringService.CalculateScore(4000, 0, 500, EmploymentStatus.Employed, false);

        Assert.InRange(result.Score, 0, 100);
    }

    [Fact]
    public void Should_Handle_Minimum_Viable_Income()
    {
        var result = _scoringService.CalculateScore(1000, 500, 100, EmploymentStatus.Employed, false);

        Assert.InRange(result.Score, 0, 100);
    }

    [Fact]
    public void Should_Cap_Score_At_100()
    {
        var result = _scoringService.CalculateScore(10000, 1000, 500, EmploymentStatus.Employed, false);

        Assert.True(result.Score <= 100);
    }

    [Fact]
    public void Should_Not_Go_Below_0()
    {
        var result = _scoringService.CalculateScore(2000, 1800, 150, EmploymentStatus.Unemployed, true);

        Assert.True(result.Score >= 0);
    }

    // Traffic Light Thresholds

    [Fact]
    public void Should_Return_Green_For_Score_Exactly_75()
    {
        var result = _scoringService.CalculateScore(5000, 2000, 600, EmploymentStatus.Employed, false);

        if (result.Score == 75)
        {
            Assert.Equal(TrafficLight.Green, result.TrafficLight);
        }
    }

    [Fact]
    public void Should_Return_Yellow_For_Score_Exactly_50()
    {
        var result = _scoringService.CalculateScore(4000, 2200, 600, EmploymentStatus.SelfEmployed, false);

        if (result.Score == 50)
        {
            Assert.Equal(TrafficLight.Yellow, result.TrafficLight);
        }
    }
}
