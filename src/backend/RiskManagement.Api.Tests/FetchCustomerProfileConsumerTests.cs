using FluentAssertions;
using MassTransit;
using Moq;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Application.Services;
using RiskManagement.Infrastructure.Sagas.Consumers;

namespace RiskManagement.Api.Tests;

public class FetchCustomerProfileConsumerTests
{
    private readonly Mock<ICustomerProfileService> _profileServiceMock = new();
    private readonly FetchCustomerProfileConsumer _consumer;

    public FetchCustomerProfileConsumerTests()
    {
        _consumer = new FetchCustomerProfileConsumer(_profileServiceMock.Object);
    }

    [Fact]
    public async Task Consume_CustomerFound_ShouldPublishCustomerProfileFetched()
    {
        var correlationId = Guid.NewGuid();
        var message = new FetchCustomerProfile(correlationId, 1);
        var contextMock = new Mock<ConsumeContext<FetchCustomerProfile>>();
        contextMock.Setup(c => c.Message).Returns(message);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        _profileServiceMock.Setup(p => p.GetCustomerProfileAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerProfile(
                1, "Max", "Mustermann", "employed", "1990-01-01",
                new CustomerAddress("Musterstraße 1", "Berlin", "10115", "Deutschland"),
                "Active"));

        await _consumer.Consume(contextMock.Object);

        contextMock.Verify(c => c.Publish(
            It.Is<CustomerProfileFetched>(e =>
                e.CorrelationId == correlationId &&
                e.FirstName == "Max" &&
                e.LastName == "Mustermann"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_CustomerNotFound_ShouldPublishApplicationCreationFailed()
    {
        var correlationId = Guid.NewGuid();
        var message = new FetchCustomerProfile(correlationId, 999);
        var contextMock = new Mock<ConsumeContext<FetchCustomerProfile>>();
        contextMock.Setup(c => c.Message).Returns(message);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        _profileServiceMock.Setup(p => p.GetCustomerProfileAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerProfile?)null);

        await _consumer.Consume(contextMock.Object);

        contextMock.Verify(c => c.Publish(
            It.Is<ApplicationCreationFailed>(e =>
                e.CorrelationId == correlationId &&
                e.Reason.Contains("nicht gefunden")),
            It.IsAny<CancellationToken>()), Times.Once);

        contextMock.Verify(c => c.Publish(
            It.IsAny<CustomerProfileFetched>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}