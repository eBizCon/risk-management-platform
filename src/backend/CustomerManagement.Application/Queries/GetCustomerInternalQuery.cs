using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Aggregates.CustomerAggregate;

namespace CustomerManagement.Application.Queries;

public record GetCustomerInternalQuery(int CustomerId) : IQuery<GetCustomerInternalResult>;

public record GetCustomerInternalResult(CustomerInternalResponse Customer);

public class GetCustomerInternalHandler : IQueryHandler<GetCustomerInternalQuery, GetCustomerInternalResult>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerInternalHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetCustomerInternalResult>> HandleAsync(GetCustomerInternalQuery query,
        CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(new CustomerId(query.CustomerId), ct);
        if (customer is null)
            return Result<GetCustomerInternalResult>.NotFound("Kunde nicht gefunden");

        return Result<GetCustomerInternalResult>.Success(
            new GetCustomerInternalResult(CustomerMapper.ToInternalResponse(customer)));
    }
}
