using FluentAssertions;
using MassTransit;
using Moq;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using RiskManagement.Infrastructure.Sagas.Consumers;
using SharedKernel.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Api.Tests;

public class FinalizeApplicationConsumerTests
{
    private readonly Mock<IApplicationRepository> _repositoryMock = new();
    private readonly Mock<IScoringConfigRepository> _configRepoMock = new();
    private readonly ScoringService _scoringService = new();
    private readonly FinalizeApplicationConsumer _consumer;

    public FinalizeApplicationConsumerTests()
    {
        _consumer = new FinalizeApplicationConsumer(
            _repositoryMock.Object,
            _configRepoMock.Object,
            _scoringService);
    }

    private static ScoringConfigVersion CreateConfigVersion()
    {
        return ScoringConfigVersion.Create(1, ScoringConfig.Default, EmailAddress.Create("admin@test.com"));
    }

    private static ApplicationEntity CreateProcessingApp()
    {
        return ApplicationEntity.CreateProcessing(
            1,
            Money.Create(5000),
            Money.Create(2000),
            Money.CreatePositive(500),
            EmailAddress.Create("user@test.com"),
            false);
    }

    private static FinalizeApplication CreateMessage(Guid correlationId, bool autoSubmit = false)
    {
        return new FinalizeApplication(correlationId,
            1,
            1,
            5000,
            2000,
            500,
            "user@test.com",
            "employed",
            false,
            420,
            DateTime.UtcNow,
            "schufa_mock",
            autoSubmit);
    }

    [Fact]
    public async Task Consume_ValidRequest_ShouldFinalizeAndPublishCompleted()
    {
        var correlationId = Guid.NewGuid();
        var app = CreateProcessingApp();
        var configVersion = CreateConfigVersion();

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<AppId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(app);
        _configRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(configVersion);

        var contextMock = new Mock<ConsumeContext<FinalizeApplication>>();
        contextMock.Setup(c => c.Message).Returns(CreateMessage(correlationId));
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await _consumer.Consume(contextMock.Object);

        app.Status.Should().Be(ApplicationStatus.Draft);
        app.Score.Should().NotBeNull();
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        contextMock.Verify(c => c.Publish(
            It.Is<ApplicationCreationCompleted>(e => e.CorrelationId == correlationId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_AutoSubmit_ShouldFinalizeAndSubmit()
    {
        var correlationId = Guid.NewGuid();
        var app = CreateProcessingApp();
        var configVersion = CreateConfigVersion();

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<AppId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(app);
        _configRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(configVersion);

        var contextMock = new Mock<ConsumeContext<FinalizeApplication>>();
        contextMock.Setup(c => c.Message).Returns(CreateMessage(correlationId, true));
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await _consumer.Consume(contextMock.Object);

        app.Status.Should().Be(ApplicationStatus.Submitted);
        contextMock.Verify(c => c.Publish(
            It.Is<ApplicationCreationCompleted>(e => e.CorrelationId == correlationId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_ApplicationNotFound_ShouldPublishFailed()
    {
        var correlationId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<AppId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationEntity?)null);

        var contextMock = new Mock<ConsumeContext<FinalizeApplication>>();
        contextMock.Setup(c => c.Message).Returns(CreateMessage(correlationId));
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await _consumer.Consume(contextMock.Object);

        contextMock.Verify(c => c.Publish(
            It.Is<ApplicationCreationFailed>(e =>
                e.CorrelationId == correlationId &&
                e.Reason.Contains("nicht gefunden")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_NoScoringConfig_ShouldPublishFailed()
    {
        var correlationId = Guid.NewGuid();
        var app = CreateProcessingApp();

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<AppId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(app);
        _configRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScoringConfigVersion?)null);

        var contextMock = new Mock<ConsumeContext<FinalizeApplication>>();
        contextMock.Setup(c => c.Message).Returns(CreateMessage(correlationId));
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await _consumer.Consume(contextMock.Object);

        contextMock.Verify(c => c.Publish(
            It.Is<ApplicationCreationFailed>(e =>
                e.CorrelationId == correlationId &&
                e.Reason.Contains("Scoring-Konfiguration")),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
