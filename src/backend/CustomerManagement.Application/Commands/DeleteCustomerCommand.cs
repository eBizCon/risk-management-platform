using CustomerManagement.Domain.Aggregates.CustomerAggregate;

namespace CustomerManagement.Application.Commands;

public record DeleteCustomerCommand(int CustomerId, string UserEmail) : ICommand<DeleteCustomerResult>;

public record DeleteCustomerResult(bool Success);

public interface IApplicationServiceClient
{
    Task<bool> HasApplicationsAsync(int customerId, CancellationToken ct = default);
}

public class DeleteCustomerHandler : ICommandHandler<DeleteCustomerCommand, DeleteCustomerResult>
{
    private readonly ICustomerRepository _repository;
    private readonly IApplicationServiceClient _applicationServiceClient;
    private readonly IDispatcher _dispatcher;

    public DeleteCustomerHandler(ICustomerRepository repository, IApplicationServiceClient applicationServiceClient,
        IDispatcher dispatcher)
    {
        _repository = repository;
        _applicationServiceClient = applicationServiceClient;
        _dispatcher = dispatcher;
    }

    public async Task<Result<DeleteCustomerResult>> HandleAsync(DeleteCustomerCommand command,
        CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(new CustomerId(command.CustomerId), ct);
        if (customer is null)
            return Result<DeleteCustomerResult>.NotFound("Kunde nicht gefunden");

        if (customer.CreatedBy != EmailAddress.Create(command.UserEmail))
            return Result<DeleteCustomerResult>.Forbidden("Zugriff verweigert");

        var hasApplications = await _applicationServiceClient.HasApplicationsAsync(command.CustomerId, ct);
        if (hasApplications)
            return Result<DeleteCustomerResult>.Failure(
                "Kunde kann nicht gelöscht werden, da noch Kreditanträge existieren");

        customer.Delete();

        await _repository.RemoveAsync(customer, ct);
        await _repository.SaveChangesAsync(ct);

        await _dispatcher.PublishDomainEventsAsync(customer, ct);

        return Result<DeleteCustomerResult>.Success(new DeleteCustomerResult(true));
    }
}