using RiskManagement.Domain.Common;

namespace RiskManagement.Domain.ValueObjects;

public sealed class ScoringResult : ValueObject
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

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Score;
        yield return TrafficLight;
        foreach (var reason in Reasons)
            yield return reason;
    }
}