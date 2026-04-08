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
        75,
        50,
        0.5m,
        0.3m,
        0.1m,
        15,
        30,
        50,
        0.3m,
        0.5m,
        0.7m,
        10,
        25,
        40,
        10,
        5,
        35,
        25,
        400,
        250,
        10,
        20);

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
            80, 40,
            0.6m, 0.4m, 0.2m,
            10, 20, 40,
            0.2m, 0.4m, 0.6m,
            5, 15, 30,
            8, 3, 30,
            20,
            450, 300, 15, 25);

        var handler = new UpdateScoringConfigHandler(_repoMock.Object);
        var command = new UpdateScoringConfigCommand(customDto, "admin@test.com");

        var result = await handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.GreenThreshold.Should().Be(80);
        result.Value.YellowThreshold.Should().Be(40);
        result.Value.PenaltySelfEmployed.Should().Be(8);
    }
}
