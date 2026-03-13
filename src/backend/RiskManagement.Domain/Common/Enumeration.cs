namespace RiskManagement.Domain.Common;

public abstract class Enumeration<T> : ValueObject where T : Enumeration<T>
{
    public string Value { get; }

    protected Enumeration(string value)
    {
        Value = value;
    }

    public static T From(string value, IReadOnlyDictionary<string, T> all)
    {
        if (all.TryGetValue(value, out var result))
            return result;

        throw new ArgumentException($"Invalid {typeof(T).Name}: '{value}'", nameof(value));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
