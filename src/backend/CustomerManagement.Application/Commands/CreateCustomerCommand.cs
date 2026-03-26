using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Aggregates.CustomerAggregate;
using CustomerManagement.Domain.ValueObjects;
using FluentValidation;
using SharedKernel.Dispatching;
using SharedKernel.ValueObjects;

namespace CustomerManagement.Application.Commands;

public record CreateCustomerCommand(CustomerCreateDto Dto, string UserEmail) : ICommand<CreateCustomerResult>;

public record CreateCustomerResult(CustomerResponse Customer);

public class CreateCustomerHandler : ICommandHandler<CreateCustomerCommand, CreateCustomerResult>
{
    private readonly ICustomerRepository _repository;
    private readonly IValidator<CustomerCreateDto> _validator;

    public CreateCustomerHandler(ICustomerRepository repository, IValidator<CustomerCreateDto> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<CreateCustomerResult>> HandleAsync(CreateCustomerCommand command,
        CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Dto, ct);
        if (!validationResult.IsValid)
        {
            var errors = ValidationHelper.ToValidationErrors(validationResult);
            return Result<CreateCustomerResult>.ValidationFailure(errors, command.Dto);
        }

        var email = string.IsNullOrWhiteSpace(command.Dto.Email) ? null : EmailAddress.Create(command.Dto.Email);

        var customer = Customer.Create(
            command.Dto.FirstName,
            command.Dto.LastName,
            email,
            PhoneNumber.Create(command.Dto.Phone),
            DateOnly.Parse(command.Dto.DateOfBirth),
            Address.Create(command.Dto.Street, command.Dto.City, command.Dto.ZipCode, command.Dto.Country),
            EmploymentStatus.From(command.Dto.EmploymentStatus),
            EmailAddress.Create(command.UserEmail));

        await _repository.AddAsync(customer, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<CreateCustomerResult>.Success(new CreateCustomerResult(CustomerMapper.ToResponse(customer)));
    }
}