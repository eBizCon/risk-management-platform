namespace RiskManagement.Domain.ReadModels;

public interface ICustomerReadModelRepository
{
    Task<List<CustomerReadModel>> GetActiveCustomersAsync(CancellationToken ct = default);
    Task<bool> IsEmptyAsync(CancellationToken ct = default);
    Task<Dictionary<int, string>> GetCustomerNamesAsync(IEnumerable<int> customerIds, CancellationToken ct = default);
}