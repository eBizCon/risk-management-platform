using FluentAssertions;
using Moq;
using RiskManagement.Application.Commands;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;
using IApplicationRepository = RiskManagement.Domain.Aggregates.ApplicationAggregate.IApplicationRepository;

namespace RiskManagement.Api.Tests;

public class RescoreOpenApplicationsHandlerTests
{
    private readonly Mock<IApplicationRepository> _appRepoMock = new();
    private readonly Mock<IScoringConfigRepository> _configRepoMock = new();
    private readonly ScoringService _scoringService = new();

    [Fact]
    public async Task HandleAsync_NoConfig_ShouldReturnFailure()
    {
        _configRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScoringConfigVersion?)null);

        var handler = new RescoreOpenApplicationsHandler(_appRepoMock.Object, _configRepoMock.Object, _scoringService);

        var result = await handler.HandleAsync(new RescoreOpenApplicationsCommand());

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Scoring-Konfiguration");
    }

    [Fact]
    public async Task HandleAsync_NoOpenApplications_ShouldReturnZeroCount()
    {
        var configVersion = ScoringConfigVersion.Create(1, ScoringConfig.Default, EmailAddress.Create("admin@test.com"));
        _configRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(configVersion);
        _appRepoMock.Setup(r => r.GetOpenApplicationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApplicationEntity>());

        var handler = new RescoreOpenApplicationsHandler(_appRepoMock.Object, _configRepoMock.Object, _scoringService);

        var result = await handler.HandleAsync(new RescoreOpenApplicationsCommand());

        result.IsSuccess.Should().BeTrue();
        result.Value!.RescoredCount.Should().Be(0);
        _appRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithOpenApplications_ShouldRescoreAndReturnCount()
    {
        var configVersion = ScoringConfigVersion.Create(1, ScoringConfig.Default, EmailAddress.Create("admin@test.com"));
        _configRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(configVersion);

        var app1 = ApplicationEntity.Create(
            1, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmploymentStatus.Employed, false, EmailAddress.Create("user@test.com"),
            _scoringService, ScoringConfig.Default, configVersion.Id);
        app1.Submit(_scoringService, ScoringConfig.Default, configVersion.Id);

        var app2 = ApplicationEntity.Create(
            2, Money.Create(3000), Money.Create(1000), Money.CreatePositive(300),
            EmploymentStatus.SelfEmployed, false, EmailAddress.Create("user@test.com"),
            _scoringService, ScoringConfig.Default, configVersion.Id);
        app2.Submit(_scoringService, ScoringConfig.Default, configVersion.Id);

        var openApps = new List<ApplicationEntity> { app1, app2 };
        _appRepoMock.Setup(r => r.GetOpenApplicationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(openApps);

        var handler = new RescoreOpenApplicationsHandler(_appRepoMock.Object, _configRepoMock.Object, _scoringService);

        var result = await handler.HandleAsync(new RescoreOpenApplicationsCommand());

        result.IsSuccess.Should().BeTrue();
        result.Value!.RescoredCount.Should().Be(2);
        _appRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
