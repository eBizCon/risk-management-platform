using FluentAssertions;
using Moq;
using RiskManagement.Application.Queries;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using SharedKernel.ValueObjects;

namespace RiskManagement.Api.Tests;

public class GetDashboardStatsHandlerTests
{
    private readonly Mock<IApplicationRepository> _repositoryMock = new();
    private readonly GetDashboardStatsHandler _handler;

    public GetDashboardStatsHandlerTests()
    {
        _handler = new GetDashboardStatsHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_AsProcessor_ShouldReturnAllStats()
    {
        _repositoryMock.Setup(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((4, 3, 3, 2));

        var result = await _handler.HandleAsync(
            new GetDashboardStatsQuery("processor@test.de", "processor"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Draft.Should().Be(4);
        result.Value.Submitted.Should().Be(3);
        result.Value.Approved.Should().Be(3);
        result.Value.Rejected.Should().Be(2);
        result.Value.Total.Should().Be(12);
    }

    [Fact]
    public async Task HandleAsync_AsApplicant_ShouldReturnUserStats()
    {
        _repositoryMock.Setup(r => r.GetUserDashboardStatsAsync(
                It.Is<EmailAddress>(e => e.Value == "user@test.de"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((2, 1, 1, 0));

        var result = await _handler.HandleAsync(
            new GetDashboardStatsQuery("user@test.de", "applicant"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Draft.Should().Be(2);
        result.Value.Submitted.Should().Be(1);
        result.Value.Approved.Should().Be(1);
        result.Value.Rejected.Should().Be(0);
        result.Value.Total.Should().Be(4);
    }

    [Fact]
    public async Task HandleAsync_WithZeroCounts_ShouldReturnZeroTotal()
    {
        _repositoryMock.Setup(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0, 0, 0));

        var result = await _handler.HandleAsync(
            new GetDashboardStatsQuery("processor@test.de", "processor"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(0);
        result.Value.Draft.Should().Be(0);
        result.Value.Submitted.Should().Be(0);
        result.Value.Approved.Should().Be(0);
        result.Value.Rejected.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_AsProcessor_ShouldNotCallUserStats()
    {
        _repositoryMock.Setup(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((1, 1, 1, 1));

        await _handler.HandleAsync(new GetDashboardStatsQuery("processor@test.de", "processor"));

        _repositoryMock.Verify(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(
            r => r.GetUserDashboardStatsAsync(It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AsApplicant_ShouldNotCallAllStats()
    {
        _repositoryMock.Setup(r => r.GetUserDashboardStatsAsync(
                It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((1, 1, 1, 1));

        await _handler.HandleAsync(new GetDashboardStatsQuery("user@test.de", "applicant"));

        _repositoryMock.Verify(
            r => r.GetUserDashboardStatsAsync(It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
