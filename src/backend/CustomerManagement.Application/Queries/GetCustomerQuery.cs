using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Aggregates.CustomerAggregate;

namespace CustomerManagement.Application.Queries;

public record GetCustomerQuery(int CustomerId, string UserEmail) : IQuery<GetCustomerResult>;

public record GetCustomerResult(CustomerResponse Customer);

public class GetCustomerHandler : IQueryHandler<GetCustomerQuery, GetCustomerResult>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetCustomerResult>> HandleAsync(GetCustomerQuery query,
        CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(new CustomerId(query.CustomerId), ct);
        if (customer is null)
            return Result<GetCustomerResult>.NotFound("Kunde nicht gefunden");

        if (customer.CreatedBy != EmailAddress.Create(query.UserEmail))
            return Result<GetCustomerResult>.Forbidden("Zugriff verweigert");

        return Result<GetCustomerResult>.Success(new GetCustomerResult(CustomerMapper.ToResponse(customer)));
    }
}
