using FluentAssertions;
using Moq;
using RiskManagement.Application.Queries;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;

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
    public async Task HandleAsync_AsApplicant_ShouldFilterByUserEmail()
    {
        // Arrange
        var email = "user@example.com";
        _repositoryMock
            .Setup(r => r.GetDashboardStatsAsync(
                It.Is<EmailAddress>(e => e.Value == email),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((2, 1, 1, 0));

        // Act
        var result = await _handler.HandleAsync(new GetDashboardStatsQuery(email, "applicant"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Draft.Should().Be(2);
        result.Value.Submitted.Should().Be(1);
        result.Value.Approved.Should().Be(1);
        result.Value.Rejected.Should().Be(0);
        result.Value.Total.Should().Be(4);

        _repositoryMock.Verify(r => r.GetDashboardStatsAsync(
            It.Is<EmailAddress>(e => e.Value == email),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AsProcessor_ShouldNotFilterByUser()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetDashboardStatsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((4, 3, 3, 2));

        // Act
        var result = await _handler.HandleAsync(new GetDashboardStatsQuery("processor@example.com", "processor"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Draft.Should().Be(4);
        result.Value.Submitted.Should().Be(3);
        result.Value.Approved.Should().Be(3);
        result.Value.Rejected.Should().Be(2);
        result.Value.Total.Should().Be(12);

        _repositoryMock.Verify(r => r.GetDashboardStatsAsync(
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNoApplications_ShouldReturnAllZeros()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetDashboardStatsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0, 0, 0));

        // Act
        var result = await _handler.HandleAsync(new GetDashboardStatsQuery("processor@example.com", "processor"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(0);
        result.Value.Draft.Should().Be(0);
        result.Value.Submitted.Should().Be(0);
        result.Value.Approved.Should().Be(0);
        result.Value.Rejected.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_TotalShouldBeSumOfFourStatuses()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetDashboardStatsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((10, 5, 3, 2));

        // Act
        var result = await _handler.HandleAsync(new GetDashboardStatsQuery("processor@example.com", "processor"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        var stats = result.Value!;
        stats.Total.Should().Be(stats.Draft + stats.Submitted + stats.Approved + stats.Rejected);
    }
}
