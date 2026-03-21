using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Aggregates.CustomerAggregate;

namespace CustomerManagement.Application.Queries;

public record GetAllCustomersInternalQuery : IQuery<List<CustomerSyncDto>>;

public record CustomerSyncDto(int Id, string FirstName, string LastName, string Status);

public class GetAllCustomersInternalHandler : IQueryHandler<GetAllCustomersInternalQuery, List<CustomerSyncDto>>
{
    private readonly ICustomerRepository _repository;

    public GetAllCustomersInternalHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<CustomerSyncDto>>> HandleAsync(GetAllCustomersInternalQuery query,
        CancellationToken ct = default)
    {
        var customers = await _repository.GetAllAsync(ct);

        var dtos = customers
            .Select(c => new CustomerSyncDto(c.Id.Value, c.FirstName, c.LastName, c.Status.Value))
            .ToList();

        return Result<List<CustomerSyncDto>>.Success(dtos);
    }
}