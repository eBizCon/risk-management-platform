namespace RiskManagement.Domain.ValueObjects;

public sealed class ScoringResult
{
    public int Score { get; }
    public TrafficLight TrafficLight { get; }
    public IReadOnlyList<string> Reasons { get; }

    public ScoringResult(int score, TrafficLight trafficLight, IReadOnlyList<string> reasons)
    {
        Score = score;
        TrafficLight = trafficLight;
        Reasons = reasons;
    }
}
