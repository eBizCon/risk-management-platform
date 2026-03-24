using MassTransit;
using Microsoft.EntityFrameworkCore;
using RiskManagement.Infrastructure.Persistence;
using SharedKernel.IntegrationEvents;

namespace RiskManagement.Infrastructure.Consumers;

public class CustomerDeletedConsumer : IConsumer<CustomerDeletedIntegrationEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerDeletedConsumer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<CustomerDeletedIntegrationEvent> context)
    {
        var existing = await _dbContext.CustomerReadModels
            .FirstOrDefaultAsync(c => c.CustomerId == context.Message.CustomerId, context.CancellationToken);

        if (existing is not null)
        {
            _dbContext.CustomerReadModels.Remove(existing);
            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}