using FluentAssertions;
using Moq;
using RiskManagement.Application.Commands;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Api.Tests;

public class UpdateScoringConfigHandlerTests
{
    private readonly Mock<IScoringConfigRepository> _repoMock = new();

    private static ScoringConfigUpdateDto ValidDto => new(
        GreenThreshold: 75,
        YellowThreshold: 50,
        IncomeRatioGood: 0.5m,
        IncomeRatioModerate: 0.3m,
        IncomeRatioLimited: 0.1m,
        PenaltyModerateRatio: 15,
        PenaltyLimitedRatio: 30,
        PenaltyCriticalRatio: 50,
        RateGood: 0.3m,
        RateModerate: 0.5m,
        RateHeavy: 0.7m,
        PenaltyModerateRate: 10,
        PenaltyHeavyRate: 25,
        PenaltyExcessiveRate: 40,
        PenaltySelfEmployed: 10,
        PenaltyRetired: 5,
        PenaltyUnemployed: 35,
        PenaltyPaymentDefault: 25);

    [Fact]
    public async Task HandleAsync_NoExistingConfig_ShouldCreateVersion1()
    {
        _repoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScoringConfigVersion?)null);

        var handler = new UpdateScoringConfigHandler(_repoMock.Object);
        var command = new UpdateScoringConfigCommand(ValidDto, "admin@test.com");

        var result = await handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Version.Should().Be(1);
        result.Value.CreatedBy.Should().Be("admin@test.com");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<ScoringConfigVersion>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ExistingConfig_ShouldIncrementVersion()
    {
        var existing = ScoringConfigVersion.Create(3, ScoringConfig.Default, EmailAddress.Create("old@test.com"));
        _repoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var handler = new UpdateScoringConfigHandler(_repoMock.Object);
        var command = new UpdateScoringConfigCommand(ValidDto, "admin@test.com");

        var result = await handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Version.Should().Be(4);
    }

    [Fact]
    public async Task HandleAsync_ValidDto_ShouldMapAllParameters()
    {
        _repoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScoringConfigVersion?)null);

        var customDto = new ScoringConfigUpdateDto(
            GreenThreshold: 80, YellowThreshold: 40,
            IncomeRatioGood: 0.6m, IncomeRatioModerate: 0.4m, IncomeRatioLimited: 0.2m,
            PenaltyModerateRatio: 10, PenaltyLimitedRatio: 20, PenaltyCriticalRatio: 40,
            RateGood: 0.2m, RateModerate: 0.4m, RateHeavy: 0.6m,
            PenaltyModerateRate: 5, PenaltyHeavyRate: 15, PenaltyExcessiveRate: 30,
            PenaltySelfEmployed: 8, PenaltyRetired: 3, PenaltyUnemployed: 30,
            PenaltyPaymentDefault: 20);

        var handler = new UpdateScoringConfigHandler(_repoMock.Object);
        var command = new UpdateScoringConfigCommand(customDto, "admin@test.com");

        var result = await handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.GreenThreshold.Should().Be(80);
        result.Value.YellowThreshold.Should().Be(40);
        result.Value.PenaltySelfEmployed.Should().Be(8);
    }
}
