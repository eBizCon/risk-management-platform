using System.Text.RegularExpressions;
using RiskManagement.Domain.Common;
using RiskManagement.Domain.Exceptions;

namespace RiskManagement.Domain.ValueObjects;

public sealed partial class EmailAddress : ValueObject
{
    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("E-Mail-Adresse darf nicht leer sein");

        if (!EmailRegex().IsMatch(value))
            throw new DomainException($"Ungültige E-Mail-Adresse: '{value}'");

        return new EmailAddress(value.Trim().ToLowerInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();
}
