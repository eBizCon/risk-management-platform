using CustomerManagement.Domain.Aggregates.CustomerAggregate;

namespace CustomerManagement.Application.DTOs;

public static class CustomerMapper
{
    public static CustomerResponse ToResponse(Customer customer)
    {
        return new CustomerResponse
        {
            Id = customer.Id.Value,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email?.Value,
            Phone = customer.Phone.Value,
            DateOfBirth = customer.DateOfBirth.ToString("yyyy-MM-dd"),
            Street = customer.Address.Street,
            City = customer.Address.City,
            ZipCode = customer.Address.ZipCode,
            Country = customer.Address.Country,
            Status = customer.Status.Value,
            CreatedBy = customer.CreatedBy.Value,
            CreatedAt = customer.CreatedAt.ToString("o"),
            UpdatedAt = customer.UpdatedAt?.ToString("o")
        };
    }

    public static CustomerInternalResponse ToInternalResponse(Customer customer)
    {
        return new CustomerInternalResponse
        {
            Id = customer.Id.Value,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Status = customer.Status.Value
        };
    }
}