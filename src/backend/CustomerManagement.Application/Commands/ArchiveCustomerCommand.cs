using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Aggregates.CustomerAggregate;

namespace CustomerManagement.Application.Commands;

public record ArchiveCustomerCommand(int CustomerId, string UserEmail) : ICommand<ArchiveCustomerResult>;

public record ArchiveCustomerResult(CustomerResponse Customer);

public class ArchiveCustomerHandler : ICommandHandler<ArchiveCustomerCommand, ArchiveCustomerResult>
{
    private readonly ICustomerRepository _repository;
    private readonly IDispatcher _dispatcher;

    public ArchiveCustomerHandler(ICustomerRepository repository, IDispatcher dispatcher)
    {
        _repository = repository;
        _dispatcher = dispatcher;
    }

    public async Task<Result<ArchiveCustomerResult>> HandleAsync(ArchiveCustomerCommand command,
        CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(new CustomerId(command.CustomerId), ct);
        if (customer is null)
            return Result<ArchiveCustomerResult>.NotFound("Kunde nicht gefunden");

        if (customer.CreatedBy != EmailAddress.Create(command.UserEmail))
            return Result<ArchiveCustomerResult>.Forbidden("Zugriff verweigert");

        customer.Archive();
        await _repository.SaveChangesAsync(ct);

        await _dispatcher.PublishDomainEventsAsync(customer, ct);

        return Result<ArchiveCustomerResult>.Success(new ArchiveCustomerResult(CustomerMapper.ToResponse(customer)));
    }
}