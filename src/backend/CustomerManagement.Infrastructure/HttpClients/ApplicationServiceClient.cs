using System.Net.Http.Json;
using CustomerManagement.Application.Commands;

namespace CustomerManagement.Infrastructure.HttpClients;

public class ApplicationServiceClient : IApplicationServiceClient
{
    private readonly HttpClient _httpClient;

    public ApplicationServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> HasApplicationsAsync(int customerId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/api/internal/applications/exists?customerId={customerId}", ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApplicationExistsResponse>(ct);
        return result?.Exists ?? false;
    }

    private record ApplicationExistsResponse(bool Exists);
}