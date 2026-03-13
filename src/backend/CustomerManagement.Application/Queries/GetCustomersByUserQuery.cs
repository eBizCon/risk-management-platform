using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Aggregates.CustomerAggregate;

namespace CustomerManagement.Application.Queries;

public record GetCustomersByUserQuery(string UserEmail) : IQuery<GetCustomersByUserResult>;

public record GetCustomersByUserResult(List<CustomerResponse> Customers);

public class GetCustomersByUserHandler : IQueryHandler<GetCustomersByUserQuery, GetCustomersByUserResult>
{
    private readonly ICustomerRepository _repository;

    public GetCustomersByUserHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetCustomersByUserResult>> HandleAsync(GetCustomersByUserQuery query,
        CancellationToken ct = default)
    {
        var customers = await _repository.GetAllByCreatorAsync(EmailAddress.Create(query.UserEmail), ct);
        var responses = customers.Select(CustomerMapper.ToResponse).ToList();
        return Result<GetCustomersByUserResult>.Success(new GetCustomersByUserResult(responses));
    }
}
