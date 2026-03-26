using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Aggregates.CustomerAggregate;

namespace CustomerManagement.Application.Commands;

public record ActivateCustomerCommand(int CustomerId, string UserEmail) : ICommand<ActivateCustomerResult>;

public record ActivateCustomerResult(CustomerResponse Customer);

public class ActivateCustomerHandler : ICommandHandler<ActivateCustomerCommand, ActivateCustomerResult>
{
    private readonly ICustomerRepository _repository;

    public ActivateCustomerHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ActivateCustomerResult>> HandleAsync(ActivateCustomerCommand command,
        CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(new CustomerId(command.CustomerId), ct);
        if (customer is null)
            return Result<ActivateCustomerResult>.NotFound("Kunde nicht gefunden");

        if (customer.CreatedBy != EmailAddress.Create(command.UserEmail))
            return Result<ActivateCustomerResult>.Forbidden("Zugriff verweigert");

        customer.Activate();
        await _repository.SaveChangesAsync(ct);

        return Result<ActivateCustomerResult>.Success(
            new ActivateCustomerResult(CustomerMapper.ToResponse(customer)));
    }
}