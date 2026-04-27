using CustomerManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RiskManagement.Domain.Services;
using RiskManagement.Infrastructure.Persistence;

var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    cancellationTokenSource.Cancel();
};

var cancellationToken = cancellationTokenSource.Token;

try
{
    var customerConnectionString = GetRequiredConnectionString("CustomerConnection");
    var riskConnectionString = GetRequiredConnectionString("RiskConnection");

    var customerDbOptions = new DbContextOptionsBuilder<CustomerDbContext>()
        .UseNpgsql(customerConnectionString)
        .Options;

    await using var customerDbContext = new CustomerDbContext(customerDbOptions);
    await customerDbContext.Database.MigrateAsync(cancellationToken);

    var customerSeeder = new CustomerManagement.Infrastructure.Seeding.DatabaseSeeder(customerDbContext);
    var customers = await customerSeeder.SeedAsync(cancellationToken);

    var riskDbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql(riskConnectionString)
        .Options;

    await using var riskDbContext = new ApplicationDbContext(riskDbOptions);
    await riskDbContext.Database.MigrateAsync(cancellationToken);

    var riskSeeder = new RiskManagement.Infrastructure.Seeding.DatabaseSeeder(riskDbContext, new ScoringService());
    var riskCustomers = customers
        .Select(customer => new RiskManagement.Infrastructure.Seeding.DatabaseSeeder.SeededCustomer(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Status))
        .ToList();

    await riskSeeder.SeedAsync(riskCustomers, cancellationToken);

    Console.WriteLine("Database seeding completed successfully.");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Database seeding was canceled.");
    return 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Database seeding failed: {exception}");
    return 1;
}

return 0;

static string GetRequiredConnectionString(string name)
{
    var value = Environment.GetEnvironmentVariable($"ConnectionStrings__{name}");

    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"Missing connection string 'ConnectionStrings__{name}'.");
    }

    return value;
}
