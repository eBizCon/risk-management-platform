using FluentAssertions;
using CustomerManagement.Domain.Aggregates.CustomerAggregate;
using CustomerManagement.Domain.Events;
using CustomerManagement.Infrastructure.IntegrationEventPublishers;
using MassTransit;
using Moq;
using SharedKernel.IntegrationEvents;

namespace RiskManagement.Api.Tests;

public class IntegrationEventPublisherTests
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();

    [Fact]
    public async Task CustomerCreatedPublisher_ShouldPublishIntegrationEvent()
    {
        var publisher = new CustomerCreatedIntegrationEventPublisher(_publishEndpointMock.Object);
        var domainEvent = new CustomerCreatedEvent(new CustomerId(1), "Max", "Mustermann");

        await publisher.HandleAsync(domainEvent);

        _publishEndpointMock.Verify(p => p.Publish(
            It.Is<CustomerCreatedIntegrationEvent>(e =>
                e.CustomerId == 1 &&
                e.FirstName == "Max" &&
                e.LastName == "Mustermann" &&
                e.Status == "active"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CustomerUpdatedPublisher_ShouldPublishIntegrationEvent()
    {
        var publisher = new CustomerUpdatedIntegrationEventPublisher(_publishEndpointMock.Object);
        var domainEvent = new CustomerUpdatedEvent(new CustomerId(2), "Anna", "Schmidt");

        await publisher.HandleAsync(domainEvent);

        _publishEndpointMock.Verify(p => p.Publish(
            It.Is<CustomerUpdatedIntegrationEvent>(e =>
                e.CustomerId == 2 &&
                e.FirstName == "Anna" &&
                e.LastName == "Schmidt" &&
                e.Status == "active"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CustomerActivatedPublisher_ShouldPublishIntegrationEvent()
    {
        var publisher = new CustomerActivatedIntegrationEventPublisher(_publishEndpointMock.Object);
        var domainEvent = new CustomerActivatedEvent(new CustomerId(3));

        await publisher.HandleAsync(domainEvent);

        _publishEndpointMock.Verify(p => p.Publish(
            It.Is<CustomerActivatedIntegrationEvent>(e => e.CustomerId == 3),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CustomerArchivedPublisher_ShouldPublishIntegrationEvent()
    {
        var publisher = new CustomerArchivedIntegrationEventPublisher(_publishEndpointMock.Object);
        var domainEvent = new CustomerArchivedEvent(new CustomerId(4));

        await publisher.HandleAsync(domainEvent);

        _publishEndpointMock.Verify(p => p.Publish(
            It.Is<CustomerArchivedIntegrationEvent>(e => e.CustomerId == 4),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CustomerDeletedPublisher_ShouldPublishIntegrationEvent()
    {
        var publisher = new CustomerDeletedIntegrationEventPublisher(_publishEndpointMock.Object);
        var domainEvent = new CustomerDeletedEvent(new CustomerId(5));

        await publisher.HandleAsync(domainEvent);

        _publishEndpointMock.Verify(p => p.Publish(
            It.Is<CustomerDeletedIntegrationEvent>(e => e.CustomerId == 5),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}