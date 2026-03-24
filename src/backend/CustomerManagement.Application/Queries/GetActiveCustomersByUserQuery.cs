using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Aggregates.CustomerAggregate;

namespace CustomerManagement.Application.Queries;

public record GetActiveCustomersByUserQuery(string UserEmail) : IQuery<GetActiveCustomersByUserResult>;

public record GetActiveCustomersByUserResult(List<CustomerResponse> Customers);

public class
    GetActiveCustomersByUserHandler : IQueryHandler<GetActiveCustomersByUserQuery, GetActiveCustomersByUserResult>
{
    private readonly ICustomerRepository _repository;

    public GetActiveCustomersByUserHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetActiveCustomersByUserResult>> HandleAsync(GetActiveCustomersByUserQuery query,
        CancellationToken ct = default)
    {
        var customers = await _repository.GetActiveByCreatorAsync(EmailAddress.Create(query.UserEmail), ct);
        var responses = customers.Select(CustomerMapper.ToResponse).ToList();
        return Result<GetActiveCustomersByUserResult>.Success(new GetActiveCustomersByUserResult(responses));
    }
}