using RiskManagement.Domain.Common;

namespace RiskManagement.Domain.ValueObjects;

public sealed class DecisionType : Enumeration<DecisionType>
{
    public static readonly DecisionType Approved = new("approved");
    public static readonly DecisionType Rejected = new("rejected");

    private static readonly Dictionary<string, DecisionType> All = new()
    {
        [Approved.Value] = Approved,
        [Rejected.Value] = Rejected
    };

    private DecisionType(string value) : base(value)
    {
    }

    public static DecisionType From(string value)
    {
        return From(value, All);
    }
}
