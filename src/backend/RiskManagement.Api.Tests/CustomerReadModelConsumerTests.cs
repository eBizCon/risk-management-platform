using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using RiskManagement.Domain.ReadModels;
using RiskManagement.Infrastructure.Consumers;
using RiskManagement.Infrastructure.Persistence;
using SharedKernel.IntegrationEvents;

namespace RiskManagement.Api.Tests;

public class CustomerReadModelConsumerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerReadModelConsumerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private static Mock<ConsumeContext<T>> CreateContext<T>(T message) where T : class
    {
        var mock = new Mock<ConsumeContext<T>>();
        mock.Setup(c => c.Message).Returns(message);
        mock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return mock;
    }

    [Fact]
    public async Task CustomerCreatedConsumer_ShouldInsertNewReadModel()
    {
        var consumer = new CustomerCreatedConsumer(_dbContext);
        var context = CreateContext(new CustomerCreatedIntegrationEvent(1, "Max", "Mustermann", "active"));

        await consumer.Consume(context.Object);

        var entry = await _dbContext.CustomerReadModels.FirstOrDefaultAsync(c => c.CustomerId == 1);
        entry.Should().NotBeNull();
        entry!.FirstName.Should().Be("Max");
        entry.LastName.Should().Be("Mustermann");
        entry.Status.Should().Be("active");
    }

    [Fact]
    public async Task CustomerCreatedConsumer_ExistingCustomer_ShouldUpsert()
    {
        _dbContext.CustomerReadModels.Add(new CustomerReadModel
        {
            CustomerId = 1, FirstName = "Old", LastName = "Name", Status = "active", LastUpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var consumer = new CustomerCreatedConsumer(_dbContext);
        var context = CreateContext(new CustomerCreatedIntegrationEvent(1, "Max", "Mustermann", "active"));

        await consumer.Consume(context.Object);

        var count = await _dbContext.CustomerReadModels.CountAsync(c => c.CustomerId == 1);
        count.Should().Be(1);
        var entry = await _dbContext.CustomerReadModels.FirstAsync(c => c.CustomerId == 1);
        entry.FirstName.Should().Be("Max");
        entry.LastName.Should().Be("Mustermann");
    }

    [Fact]
    public async Task CustomerUpdatedConsumer_ExistingCustomer_ShouldUpdate()
    {
        _dbContext.CustomerReadModels.Add(new CustomerReadModel
        {
            CustomerId = 2, FirstName = "Old", LastName = "Name", Status = "active", LastUpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var consumer = new CustomerUpdatedConsumer(_dbContext);
        var context = CreateContext(new CustomerUpdatedIntegrationEvent(2, "Anna", "Schmidt", "active"));

        await consumer.Consume(context.Object);

        var entry = await _dbContext.CustomerReadModels.FirstAsync(c => c.CustomerId == 2);
        entry.FirstName.Should().Be("Anna");
        entry.LastName.Should().Be("Schmidt");
    }

    [Fact]
    public async Task CustomerUpdatedConsumer_NonExistingCustomer_ShouldNotThrow()
    {
        var consumer = new CustomerUpdatedConsumer(_dbContext);
        var context = CreateContext(new CustomerUpdatedIntegrationEvent(999, "Ghost", "User", "active"));

        var act = () => consumer.Consume(context.Object);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CustomerActivatedConsumer_ShouldSetStatusActive()
    {
        _dbContext.CustomerReadModels.Add(new CustomerReadModel
        {
            CustomerId = 3, FirstName = "Max", LastName = "Mustermann", Status = "archived",
            LastUpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var consumer = new CustomerActivatedConsumer(_dbContext);
        var context = CreateContext(new CustomerActivatedIntegrationEvent(3));

        await consumer.Consume(context.Object);

        var entry = await _dbContext.CustomerReadModels.FirstAsync(c => c.CustomerId == 3);
        entry.Status.Should().Be("active");
    }

    [Fact]
    public async Task CustomerArchivedConsumer_ShouldSetStatusArchived()
    {
        _dbContext.CustomerReadModels.Add(new CustomerReadModel
        {
            CustomerId = 4, FirstName = "Max", LastName = "Mustermann", Status = "active",
            LastUpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var consumer = new CustomerArchivedConsumer(_dbContext);
        var context = CreateContext(new CustomerArchivedIntegrationEvent(4));

        await consumer.Consume(context.Object);

        var entry = await _dbContext.CustomerReadModels.FirstAsync(c => c.CustomerId == 4);
        entry.Status.Should().Be("archived");
    }

    [Fact]
    public async Task CustomerDeletedConsumer_ShouldRemoveReadModel()
    {
        _dbContext.CustomerReadModels.Add(new CustomerReadModel
        {
            CustomerId = 5, FirstName = "Max", LastName = "Mustermann", Status = "active",
            LastUpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var consumer = new CustomerDeletedConsumer(_dbContext);
        var context = CreateContext(new CustomerDeletedIntegrationEvent(5));

        await consumer.Consume(context.Object);

        var entry = await _dbContext.CustomerReadModels.FirstOrDefaultAsync(c => c.CustomerId == 5);
        entry.Should().BeNull();
    }

    [Fact]
    public async Task CustomerDeletedConsumer_NonExistingCustomer_ShouldNotThrow()
    {
        var consumer = new CustomerDeletedConsumer(_dbContext);
        var context = CreateContext(new CustomerDeletedIntegrationEvent(999));

        var act = () => consumer.Consume(context.Object);

        await act.Should().NotThrowAsync();
    }
}