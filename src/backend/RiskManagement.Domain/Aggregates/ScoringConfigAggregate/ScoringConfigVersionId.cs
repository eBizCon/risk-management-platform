namespace RiskManagement.Domain.Aggregates.ScoringConfigAggregate;

public readonly record struct ScoringConfigVersionId(int Value)
{
    public override string ToString() => Value.ToString();
}
