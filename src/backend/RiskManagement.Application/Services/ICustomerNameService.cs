namespace RiskManagement.Application.Services;

public interface ICustomerNameService
{
    Task<string?> GetCustomerNameAsync(int customerId, CancellationToken ct = default);
    Task<Dictionary<int, string>> GetCustomerNamesAsync(IEnumerable<int> customerIds, CancellationToken ct = default);
}
