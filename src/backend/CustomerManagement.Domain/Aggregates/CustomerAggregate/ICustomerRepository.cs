namespace CustomerManagement.Domain.Aggregates.CustomerAggregate;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken ct = default);
    Task<List<Customer>> GetAllAsync(CancellationToken ct = default);
    Task<List<Customer>> GetAllByCreatorAsync(EmailAddress createdBy, CancellationToken ct = default);
    Task<List<Customer>> GetActiveByCreatorAsync(EmailAddress createdBy, CancellationToken ct = default);
    Task AddAsync(Customer customer, CancellationToken ct = default);
    Task RemoveAsync(Customer customer, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}