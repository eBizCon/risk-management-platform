using Microsoft.EntityFrameworkCore;
using RiskManagement.Domain.ReadModels;

namespace RiskManagement.Infrastructure.Persistence;

public class CustomerReadModelRepository : ICustomerReadModelRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerReadModelRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CustomerReadModel>> GetActiveCustomersAsync(CancellationToken ct = default)
    {
        return await _dbContext.CustomerReadModels
            .Where(c => c.Status == "active")
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(ct);
    }

    public async Task<bool> IsEmptyAsync(CancellationToken ct = default)
    {
        return !await _dbContext.CustomerReadModels.AnyAsync(ct);
    }

    public async Task<Dictionary<int, string>> GetCustomerNamesAsync(IEnumerable<int> customerIds,
        CancellationToken ct = default)
    {
        var idList = customerIds.Distinct().ToList();
        return await _dbContext.CustomerReadModels
            .Where(c => idList.Contains(c.CustomerId))
            .ToDictionaryAsync(c => c.CustomerId, c => $"{c.FirstName} {c.LastName}", ct);
    }
}