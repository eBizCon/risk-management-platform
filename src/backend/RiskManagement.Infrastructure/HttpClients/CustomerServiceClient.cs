using System.Net.Http.Json;
using System.Text.Json;
using RiskManagement.Application.Services;

namespace RiskManagement.Infrastructure.HttpClients;

public class CustomerServiceClient : ICustomerNameService, ICustomerProfileService
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

    public async Task<string?> GetCustomerNameAsync(int customerId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/internal/customers/{customerId}", ct);
            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<CustomerInternalResult>(JsonOptions, ct);
            if (result?.Customer is null)
                return null;

            return $"{result.Customer.LastName}, {result.Customer.FirstName}";
        }
        catch
        {
            return null;
        }
    }

    public async Task<Dictionary<int, string>> GetCustomerNamesAsync(IEnumerable<int> customerIds,
        CancellationToken ct = default)
    {
        var names = new Dictionary<int, string>();
        foreach (var id in customerIds.Distinct())
        {
            var name = await GetCustomerNameAsync(id, ct);
            if (name is not null)
                names[id] = name;
        }

        return names;
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

            var cr = result.Customer.CreditReport;
            return new CustomerProfile(
                result.Customer.Id,
                result.Customer.FirstName,
                result.Customer.LastName,
                result.Customer.EmploymentStatus,
                cr is not null
                    ? new CustomerCreditReport(cr.HasPaymentDefault, cr.CreditScore, cr.CheckedAt, cr.Provider)
                    : null,
                result.Customer.Status);
        }
        catch
        {
            return null;
        }
    }

    private record CreditReportInternalDto(
        bool HasPaymentDefault,
        int? CreditScore,
        string CheckedAt,
        string Provider);

    private record CustomerInternalDto(
        int Id,
        string FirstName,
        string LastName,
        string EmploymentStatus,
        CreditReportInternalDto? CreditReport,
        string Status);

    private record CustomerInternalResult(CustomerInternalDto Customer);
}