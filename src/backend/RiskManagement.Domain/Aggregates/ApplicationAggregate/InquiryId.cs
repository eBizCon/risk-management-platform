namespace RiskManagement.Domain.Aggregates.ApplicationAggregate;

public readonly record struct InquiryId(int Value)
{
    public override string ToString()
    {
        return Value.ToString();
    }
}