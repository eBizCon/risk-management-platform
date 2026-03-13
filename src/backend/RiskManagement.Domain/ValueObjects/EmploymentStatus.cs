namespace RiskManagement.Domain.ValueObjects;

public sealed class EmploymentStatus : IEquatable<EmploymentStatus>
{
    public static readonly EmploymentStatus Employed = new("employed");
    public static readonly EmploymentStatus SelfEmployed = new("self_employed");
    public static readonly EmploymentStatus Unemployed = new("unemployed");
    public static readonly EmploymentStatus Retired = new("retired");

    private static readonly Dictionary<string, EmploymentStatus> All = new()
    {
        [Employed.Value] = Employed,
        [SelfEmployed.Value] = SelfEmployed,
        [Unemployed.Value] = Unemployed,
        [Retired.Value] = Retired
    };

    public string Value { get; }

    private EmploymentStatus(string value)
    {
        Value = value;
    }

    public static EmploymentStatus From(string value)
    {
        if (All.TryGetValue(value, out var status))
            return status;

        throw new ArgumentException($"Invalid EmploymentStatus: '{value}'", nameof(value));
    }

    public static string[] AllValues => All.Keys.ToArray();

    public override string ToString()
    {
        return Value;
    }

    public bool Equals(EmploymentStatus? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is EmploymentStatus other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(EmploymentStatus? left, EmploymentStatus? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(EmploymentStatus? left, EmploymentStatus? right)
    {
        return !(left == right);
    }
}