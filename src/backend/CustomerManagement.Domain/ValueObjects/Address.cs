namespace CustomerManagement.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string ZipCode { get; }
    public string Country { get; }

    private Address(string street, string city, string zipCode, string country)
    {
        Street = street;
        City = city;
        ZipCode = zipCode;
        Country = country;
    }

    public static Address Create(string street, string city, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Straße darf nicht leer sein");
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("Stadt darf nicht leer sein");
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new DomainException("PLZ darf nicht leer sein");
        if (string.IsNullOrWhiteSpace(country))
            throw new DomainException("Land darf nicht leer sein");

        return new Address(street.Trim(), city.Trim(), zipCode.Trim(), country.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return ZipCode;
        yield return Country;
    }

    public override string ToString() => $"{Street}, {ZipCode} {City}, {Country}";
}
