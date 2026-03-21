using MassTransit;
using Microsoft.EntityFrameworkCore;
using RiskManagement.Infrastructure.Persistence;
using SharedKernel.IntegrationEvents;

namespace RiskManagement.Infrastructure.Consumers;

public class CustomerUpdatedConsumer : IConsumer<CustomerUpdatedIntegrationEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerUpdatedConsumer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<CustomerUpdatedIntegrationEvent> context)
    {
        var msg = context.Message;

        var existing = await _dbContext.CustomerReadModels
            .FirstOrDefaultAsync(c => c.CustomerId == msg.CustomerId, context.CancellationToken);

        if (existing is not null)
        {
            existing.FirstName = msg.FirstName;
            existing.LastName = msg.LastName;
            existing.Status = msg.Status;
            existing.LastUpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}