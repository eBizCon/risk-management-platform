using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RiskManagement.Domain.ReadModels;
using RiskManagement.Infrastructure.Persistence;

namespace RiskManagement.Infrastructure.Services;

public class CustomerReadModelSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerReadModelSyncService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CustomerReadModelSyncService(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        ILogger<CustomerReadModelSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClient = httpClientFactory.CreateClient("CustomerSyncClient");
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var isEmpty = !await dbContext.CustomerReadModels.AnyAsync(stoppingToken);
            if (!isEmpty)
            {
                _logger.LogInformation("CustomerReadModel table is not empty, skipping initial sync");
                return;
            }

            _logger.LogInformation(
                "CustomerReadModel table is empty, starting initial sync from CustomerManagement API");

            var response = await _httpClient.GetAsync("/api/internal/customers", stoppingToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch customers from CustomerManagement API: {StatusCode}",
                    response.StatusCode);
                return;
            }

            var customers = await response.Content.ReadFromJsonAsync<List<CustomerSyncDto>>(JsonOptions, stoppingToken);
            if (customers is null || customers.Count == 0)
            {
                _logger.LogInformation("No customers returned from CustomerManagement API");
                return;
            }

            foreach (var customer in customers)
                dbContext.CustomerReadModels.Add(new CustomerReadModel
                {
                    CustomerId = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Status = customer.Status,
                    LastUpdatedAt = DateTime.UtcNow
                });

            await dbContext.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Initial sync completed: {Count} customers synced", customers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during initial CustomerReadModel sync");
        }
    }

    private record CustomerSyncDto(int Id, string FirstName, string LastName, string Status);
}