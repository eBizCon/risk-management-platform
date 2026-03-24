using System.Net.Http.Json;
using System.Text.Json;
using RiskManagement.Application.Services;

namespace RiskManagement.Infrastructure.HttpClients;

public class CustomerServiceClient : ICustomerProfileService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CustomerServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CustomerProfile?> GetCustomerProfileAsync(int customerId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/internal/customers/{customerId}", ct);
            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<CustomerInternalResult>(JsonOptions, ct);
            if (result?.Customer is null)
                return null;

            return new CustomerProfile(
                result.Customer.Id,
                result.Customer.FirstName,
                result.Customer.LastName,
                result.Customer.EmploymentStatus,
                result.Customer.DateOfBirth,
                new CustomerAddress(
                    result.Customer.Street,
                    result.Customer.City,
                    result.Customer.ZipCode,
                    result.Customer.Country),
                result.Customer.Status);
        }
        catch
        {
            return null;
        }
    }

    private record CustomerInternalDto(
        int Id,
        string FirstName,
        string LastName,
        string EmploymentStatus,
        string DateOfBirth,
        string Street,
        string City,
        string ZipCode,
        string Country,
        string Status);

    private record CustomerInternalResult(CustomerInternalDto Customer);
}