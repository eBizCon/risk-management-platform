using RiskManagement.Application.Common;
using RiskManagement.Domain.ReadModels;

namespace RiskManagement.Application.Queries;

public record GetActiveCustomersQuery : IQuery<List<CustomerReadModelDto>>;

public record CustomerReadModelDto(int CustomerId, string FirstName, string LastName);

public class GetActiveCustomersHandler : IQueryHandler<GetActiveCustomersQuery, List<CustomerReadModelDto>>
{
    private readonly ICustomerReadModelRepository _repository;

    public GetActiveCustomersHandler(ICustomerReadModelRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<CustomerReadModelDto>>> HandleAsync(GetActiveCustomersQuery query,
        CancellationToken ct = default)
    {
        var customers = await _repository.GetActiveCustomersAsync(ct);

        var dtos = customers
            .Select(c => new CustomerReadModelDto(c.CustomerId, c.FirstName, c.LastName))
            .ToList();

        return Result<List<CustomerReadModelDto>>.Success(dtos);
    }
}