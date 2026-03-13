using System.Text.RegularExpressions;

namespace CustomerManagement.Domain.ValueObjects;

public sealed partial class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Telefonnummer darf nicht leer sein");

        var normalized = value.Trim();
        if (!PhoneRegex().IsMatch(normalized))
            throw new DomainException($"Ungültige Telefonnummer: '{value}'");

        return new PhoneNumber(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^\+?[\d\s\-/()]{5,20}$")]
    private static partial Regex PhoneRegex();
}
