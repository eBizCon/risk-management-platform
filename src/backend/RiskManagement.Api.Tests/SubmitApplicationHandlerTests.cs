using FluentAssertions;
using MassTransit;
using Moq;
using RiskManagement.Application.Commands;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Api.Tests;

public class SubmitApplicationHandlerTests
{
    private readonly Mock<IApplicationRepository> _repositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly SubmitApplicationHandler _handler;

    private const string UserEmail = "user@test.com";

    public SubmitApplicationHandlerTests()
    {
        _handler = new SubmitApplicationHandler(
            _repositoryMock.Object,
            _publishEndpointMock.Object);
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
            EmploymentStatus.Employed,
            CreditReport.Create(false, 420, DateTime.UtcNow, "schufa_mock"),
            EmailAddress.Create(UserEmail),
            scoringService,
            config,
            configVersion.Id);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ShouldSetProcessingAndPublishWithAutoSubmitTrue()
    {
        var app = CreateDraftApp();
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<AppId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(app);

        var command = new SubmitApplicationCommand(1, UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("processing");
        app.Status.Should().Be(ApplicationStatus.Processing);

        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(p => p.Publish(
            It.Is<ApplicationUpdateStarted>(e =>
                e.CustomerId == 1 &&
                e.Income == 5000 &&
                e.AutoSubmit == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ApplicationNotFound_ShouldReturnNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<AppId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationEntity?)null);

        var command = new SubmitApplicationCommand(1, UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<ApplicationUpdateStarted>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WrongUser_ShouldReturnForbidden()
    {
        var app = CreateDraftApp();
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<AppId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(app);

        var command = new SubmitApplicationCommand(1, "other@test.com");
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<ApplicationUpdateStarted>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
