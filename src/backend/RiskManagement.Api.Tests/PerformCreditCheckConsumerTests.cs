using FluentAssertions;
using MassTransit;
using Moq;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using RiskManagement.Infrastructure.Sagas.Consumers;

namespace RiskManagement.Api.Tests;

public class PerformCreditCheckConsumerTests
{
    private readonly Mock<ICreditCheckService> _creditCheckServiceMock = new();
    private readonly PerformCreditCheckConsumer _consumer;

    public PerformCreditCheckConsumerTests()
    {
        _consumer = new PerformCreditCheckConsumer(_creditCheckServiceMock.Object);
    }

    [Fact]
    public async Task Consume_ValidRequest_ShouldPublishCreditCheckCompleted()
    {
        var correlationId = Guid.NewGuid();
        var message = new PerformCreditCheck(
            correlationId, "Max", "Mustermann",
            DateOnly.Parse("1990-01-01"),
            "Musterstraße 1", "Berlin", "10115", "Deutschland");

        var contextMock = new Mock<ConsumeContext<PerformCreditCheck>>();
        contextMock.Setup(c => c.Message).Returns(message);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        var checkedAt = DateTime.UtcNow;
        _creditCheckServiceMock.Setup(c => c.CheckAsync(
                "Max", "Mustermann", DateOnly.Parse("1990-01-01"),
                "Musterstraße 1", "Berlin", "10115", "Deutschland"))
            .ReturnsAsync(CreditCheckResult.Create(false, 420, checkedAt, "schufa_mock"));

        await _consumer.Consume(contextMock.Object);

        contextMock.Verify(c => c.Publish(
            It.Is<CreditCheckCompleted>(e =>
                e.CorrelationId == correlationId &&
                e.HasPaymentDefault == false &&
                e.CreditScore == 420 &&
                e.Provider == "schufa_mock"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}