using RiskManagement.Domain.Common;
using RiskManagement.Domain.Exceptions;

namespace RiskManagement.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }

    private Money(decimal amount)
    {
        Amount = amount;
    }

    public static Money Create(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("Geldbetrag darf nicht negativ sein");

        return new Money(amount);
    }

    public static Money CreatePositive(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Geldbetrag muss positiv sein");

        return new Money(amount);
    }

    public static Money Zero => new(0m);

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);
    public static Money operator -(Money left, Money right) => new(left.Amount - right.Amount);
    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;
    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;
    public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;
    public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }

    public override string ToString() => Amount.ToString("F2");
}
