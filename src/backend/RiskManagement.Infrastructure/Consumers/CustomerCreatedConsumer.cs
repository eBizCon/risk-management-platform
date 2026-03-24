using MassTransit;
using Microsoft.EntityFrameworkCore;
using RiskManagement.Domain.ReadModels;
using RiskManagement.Infrastructure.Persistence;
using SharedKernel.IntegrationEvents;

namespace RiskManagement.Infrastructure.Consumers;

public class CustomerCreatedConsumer : IConsumer<CustomerCreatedIntegrationEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerCreatedConsumer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<CustomerCreatedIntegrationEvent> context)
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
        }
        else
        {
            _dbContext.CustomerReadModels.Add(new CustomerReadModel
            {
                CustomerId = msg.CustomerId,
                FirstName = msg.FirstName,
                LastName = msg.LastName,
                Status = msg.Status,
                LastUpdatedAt = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}