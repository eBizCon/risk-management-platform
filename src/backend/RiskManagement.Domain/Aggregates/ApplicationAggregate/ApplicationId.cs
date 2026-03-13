namespace RiskManagement.Domain.Aggregates.ApplicationAggregate;

public readonly record struct ApplicationId(int Value)
{
    public override string ToString() => Value.ToString();
}
