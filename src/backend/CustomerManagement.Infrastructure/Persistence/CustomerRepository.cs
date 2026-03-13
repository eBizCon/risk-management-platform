using CustomerManagement.Domain.Aggregates.CustomerAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel.ValueObjects;

namespace CustomerManagement.Infrastructure.Persistence;

public class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDbContext _context;

    public CustomerRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken ct = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<List<Customer>> GetAllByCreatorAsync(EmailAddress createdBy, CancellationToken ct = default)
    {
        return await _context.Customers
            .Where(c => c.CreatedBy == createdBy)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Customer>> GetActiveByCreatorAsync(EmailAddress createdBy, CancellationToken ct = default)
    {
        return await _context.Customers
            .Where(c => c.CreatedBy == createdBy && c.Status == CustomerStatus.Active)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
    {
        await _context.Customers.AddAsync(customer, ct);
    }

    public Task RemoveAsync(Customer customer, CancellationToken ct = default)
    {
        _context.Customers.Remove(customer);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
