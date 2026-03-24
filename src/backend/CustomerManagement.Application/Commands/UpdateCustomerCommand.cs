using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Aggregates.CustomerAggregate;
using CustomerManagement.Domain.ValueObjects;
using FluentValidation;
using SharedKernel.ValueObjects;

namespace CustomerManagement.Application.Commands;

public record UpdateCustomerCommand(int CustomerId, CustomerUpdateDto Dto, string UserEmail)
    : ICommand<UpdateCustomerResult>;

public record UpdateCustomerResult(CustomerResponse Customer);

public class UpdateCustomerHandler : ICommandHandler<UpdateCustomerCommand, UpdateCustomerResult>
{
    private readonly ICustomerRepository _repository;
    private readonly IValidator<CustomerUpdateDto> _validator;
    private readonly IDispatcher _dispatcher;

    public UpdateCustomerHandler(ICustomerRepository repository, IValidator<CustomerUpdateDto> validator,
        IDispatcher dispatcher)
    {
        _repository = repository;
        _validator = validator;
        _dispatcher = dispatcher;
    }

    public async Task<Result<UpdateCustomerResult>> HandleAsync(UpdateCustomerCommand command,
        CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(new CustomerId(command.CustomerId), ct);
        if (customer is null)
            return Result<UpdateCustomerResult>.NotFound("Kunde nicht gefunden");

        if (customer.CreatedBy != EmailAddress.Create(command.UserEmail))
            return Result<UpdateCustomerResult>.Forbidden("Zugriff verweigert");

        var validationResult = await _validator.ValidateAsync(command.Dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = ValidationHelper.ToValidationErrors(validationResult);
            return Result<UpdateCustomerResult>.ValidationFailure(errors, command.Dto);
        }

        var email = string.IsNullOrWhiteSpace(command.Dto.Email) ? null : EmailAddress.Create(command.Dto.Email);

        customer.Update(
            command.Dto.FirstName,
            command.Dto.LastName,
            email,
            PhoneNumber.Create(command.Dto.Phone),
            DateOnly.Parse(command.Dto.DateOfBirth),
            Address.Create(command.Dto.Street, command.Dto.City, command.Dto.ZipCode, command.Dto.Country),
            EmploymentStatus.From(command.Dto.EmploymentStatus));

        await _repository.SaveChangesAsync(ct);

        await _dispatcher.PublishDomainEventsAsync(customer, ct);

        return Result<UpdateCustomerResult>.Success(new UpdateCustomerResult(CustomerMapper.ToResponse(customer)));
    }
}