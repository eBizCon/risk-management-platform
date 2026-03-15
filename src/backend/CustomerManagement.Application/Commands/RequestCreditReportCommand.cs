using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Aggregates.CustomerAggregate;
using CustomerManagement.Domain.Services;
using CustomerManagement.Domain.ValueObjects;

namespace CustomerManagement.Application.Commands;

public record RequestCreditReportCommand(int CustomerId, string UserEmail) : ICommand<RequestCreditReportResult>;

public record RequestCreditReportResult(CustomerResponse Customer);

public class RequestCreditReportHandler : ICommandHandler<RequestCreditReportCommand, RequestCreditReportResult>
{
    private readonly ICustomerRepository _repository;
    private readonly ICreditReportProvider _creditReportProvider;

    public RequestCreditReportHandler(ICustomerRepository repository, ICreditReportProvider creditReportProvider)
    {
        _repository = repository;
        _creditReportProvider = creditReportProvider;
    }

    public async Task<Result<RequestCreditReportResult>> HandleAsync(RequestCreditReportCommand command,
        CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(new CustomerId(command.CustomerId), ct);
        if (customer is null)
            return Result<RequestCreditReportResult>.NotFound("Kunde nicht gefunden");

        if (customer.CreatedBy != EmailAddress.Create(command.UserEmail))
            return Result<RequestCreditReportResult>.Forbidden("Zugriff verweigert");

        var creditReport = await _creditReportProvider.CheckAsync(
            customer.FirstName,
            customer.LastName,
            customer.DateOfBirth,
            customer.Address);

        customer.UpdateCreditReport(creditReport);
        await _repository.SaveChangesAsync(ct);

        return Result<RequestCreditReportResult>.Success(
            new RequestCreditReportResult(CustomerMapper.ToResponse(customer)));
    }
}
