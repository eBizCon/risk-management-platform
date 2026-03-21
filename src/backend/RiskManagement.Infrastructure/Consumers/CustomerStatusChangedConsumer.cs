using MassTransit;
using Microsoft.EntityFrameworkCore;
using RiskManagement.Infrastructure.Persistence;
using SharedKernel.IntegrationEvents;

namespace RiskManagement.Infrastructure.Consumers;

public class CustomerActivatedConsumer : IConsumer<CustomerActivatedIntegrationEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerActivatedConsumer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<CustomerActivatedIntegrationEvent> context)
    {
        var existing = await _dbContext.CustomerReadModels
            .FirstOrDefaultAsync(c => c.CustomerId == context.Message.CustomerId, context.CancellationToken);

        if (existing is not null)
        {
            existing.Status = "active";
            existing.LastUpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}

public class CustomerArchivedConsumer : IConsumer<CustomerArchivedIntegrationEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerArchivedConsumer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<CustomerArchivedIntegrationEvent> context)
    {
        var existing = await _dbContext.CustomerReadModels
            .FirstOrDefaultAsync(c => c.CustomerId == context.Message.CustomerId, context.CancellationToken);

        if (existing is not null)
        {
            existing.Status = "archived";
            existing.LastUpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}