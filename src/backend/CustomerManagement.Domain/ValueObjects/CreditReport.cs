namespace CustomerManagement.Domain.ValueObjects;

public sealed class CreditReport : ValueObject
{
    public bool HasPaymentDefault { get; }
    public int? CreditScore { get; }
    public DateTime CheckedAt { get; }
    public string Provider { get; }

    private CreditReport(bool hasPaymentDefault, int? creditScore, DateTime checkedAt, string provider)
    {
        HasPaymentDefault = hasPaymentDefault;
        CreditScore = creditScore;
        CheckedAt = checkedAt;
        Provider = provider;
    }

    public static CreditReport Create(bool hasPaymentDefault, int? creditScore, DateTime checkedAt, string provider)
    {
        if (creditScore is < 100 or > 600)
            throw new DomainException("CreditScore muss zwischen 100 und 600 liegen");

        if (string.IsNullOrWhiteSpace(provider))
            throw new DomainException("Provider darf nicht leer sein");

        return new CreditReport(hasPaymentDefault, creditScore, checkedAt, provider);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return HasPaymentDefault;
        yield return CreditScore;
        yield return CheckedAt;
        yield return Provider;
    }
}
