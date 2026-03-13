namespace CustomerManagement.Domain.Aggregates.CustomerAggregate;

public sealed class CustomerStatus : Enumeration<CustomerStatus>
{
    public static readonly CustomerStatus Active = new("active");
    public static readonly CustomerStatus Archived = new("archived");

    private static readonly IReadOnlyDictionary<string, CustomerStatus> All =
        new Dictionary<string, CustomerStatus>
        {
            { Active.Value, Active },
            { Archived.Value, Archived }
        };

    private CustomerStatus(string value) : base(value)
    {
    }

    public static CustomerStatus From(string value)
    {
        return From(value, All);
    }
}