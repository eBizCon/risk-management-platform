using FluentAssertions;
using MassTransit;
using Moq;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using RiskManagement.Infrastructure.Sagas.Consumers;
using SharedKernel.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Api.Tests;

public class MarkApplicationFailedConsumerTests
{
    private readonly Mock<IApplicationRepository> _repositoryMock = new();
    private readonly MarkApplicationFailedConsumer _consumer;

    public MarkApplicationFailedConsumerTests()
    {
        _consumer = new MarkApplicationFailedConsumer(_repositoryMock.Object);
    }

    private static ApplicationEntity CreateProcessingApp()
    {
        return ApplicationEntity.CreateProcessing(
            1,
            Money.Create(5000),
            Money.Create(2000),
            Money.CreatePositive(500),
            Money.CreatePositive(25000),
            48,
            EmailAddress.Create("user@test.com"));
    }

    private static ApplicationEntity CreateDraftApp()
    {
        var scoringService = new ScoringService();
        var config = ScoringConfig.Default;
        var configVersion = ScoringConfigVersion.Create(1, config, EmailAddress.Create("admin@test.com"));

        return ApplicationEntity.Create(
            1,
            Money.Create(5000),
            Money.Create(2000),
            Money.CreatePositive(500),
            Money.CreatePositive(25000),
            48,
            EmploymentStatus.Employed,
            CreditReport.Create(false, 420, DateTime.UtcNow, "schufa_mock"),
            EmailAddress.Create("user@test.com"),
            scoringService,
            config,
            configVersion.Id);
    }

    [Fact]
    public async Task Consume_ProcessingApplication_ShouldMarkAsFailed()
    {
        var app = CreateProcessingApp();
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<AppId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(app);

        var contextMock = new Mock<ConsumeContext<MarkApplicationFailed>>();
        contextMock.Setup(c => c.Message).Returns(
            new MarkApplicationFailed(Guid.NewGuid(), 1, "Customer profile fetch failed"));
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await _consumer.Consume(contextMock.Object);

        app.Status.Should().Be(ApplicationStatus.Failed);
        app.FailureReason.Should().Be("Customer profile fetch failed");
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_ApplicationNotFound_ShouldDoNothing()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<AppId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationEntity?)null);

        var contextMock = new Mock<ConsumeContext<MarkApplicationFailed>>();
        contextMock.Setup(c => c.Message).Returns(
            new MarkApplicationFailed(Guid.NewGuid(), 999, "some error"));
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await _consumer.Consume(contextMock.Object);

        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_AlreadyFailedOrDraft_ShouldSkip()
    {
        var app = CreateDraftApp();
        app.Status.Should().Be(ApplicationStatus.Draft);

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<AppId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(app);

        var contextMock = new Mock<ConsumeContext<MarkApplicationFailed>>();
        contextMock.Setup(c => c.Message).Returns(
            new MarkApplicationFailed(Guid.NewGuid(), 1, "some error"));
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await _consumer.Consume(contextMock.Object);

        app.Status.Should().Be(ApplicationStatus.Draft);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
