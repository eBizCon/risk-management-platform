using CustomerManagement.Domain.Aggregates.CustomerAggregate;
using CustomerManagement.Domain.ValueObjects;
using CustomerManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.ValueObjects;

namespace CustomerManagement.Infrastructure.Seeding;

public class DatabaseSeeder
{
    public sealed record SeededCustomer(int Id, string FirstName, string LastName, string Status);

    private readonly CustomerDbContext _context;

    private const string SeedCreatedBy = "applicant@example.com";

    private static readonly (string FirstName, string LastName, EmploymentStatus EmploymentStatus)[] Templates =
    {
        ("Max", "Mustermann", EmploymentStatus.Employed),
        ("Sofia", "Wagner", EmploymentStatus.SelfEmployed),
        ("Jonas", "Becker", EmploymentStatus.Employed),
        ("Elena", "Fischer", EmploymentStatus.Unemployed),
        ("Noah", "Klein", EmploymentStatus.Employed),
        ("Mila", "Schmitt", EmploymentStatus.Retired),
        ("Paul", "Neumann", EmploymentStatus.SelfEmployed),
        ("Lea", "Hartmann", EmploymentStatus.Unemployed)
    };

    public DatabaseSeeder(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SeededCustomer>> SeedAsync(CancellationToken ct = default)
    {
        var existingCustomers = (await _context.Customers
            .Select(customer => new SeededCustomer(
                customer.Id.Value,
                customer.FirstName,
                customer.LastName,
                customer.Status.Value))
            .ToListAsync(ct))
            .OrderBy(customer => customer.Id)
            .ToList();

        if (existingCustomers.Count > 0)
        {
            return existingCustomers;
        }

        var createdBy = EmailAddress.Create(SeedCreatedBy);

        for (var index = 0; index < Templates.Length; index++)
        {
            var template = Templates[index];
            var firstName = template.FirstName;
            var lastName = template.LastName;

            var customer = Customer.Create(
                firstName,
                lastName,
                EmailAddress.Create($"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}@example.com"),
                PhoneNumber.Create($"+49 151 {1000000 + index:D7}"),
                new DateOnly(1980 + index, (index % 12) + 1, (index % 28) + 1),
                Address.Create(
                    $"Musterstraße {index + 1}",
                    "München",
                    $"80{index + 10:D3}",
                    "Deutschland"),
                template.EmploymentStatus,
                createdBy);

            _context.Customers.Add(customer);
        }

        await _context.SaveChangesAsync(ct);

        return (await _context.Customers
            .Select(customer => new SeededCustomer(
                customer.Id.Value,
                customer.FirstName,
                customer.LastName,
                customer.Status.Value))
            .ToListAsync(ct))
            .OrderBy(customer => customer.Id)
            .ToList();
    }
}
