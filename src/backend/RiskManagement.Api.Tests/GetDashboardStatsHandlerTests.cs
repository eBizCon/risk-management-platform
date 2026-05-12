using FluentAssertions;
using Moq;
using RiskManagement.Application.Queries;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;

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
    public async Task HandleAsync_ApplicantRole_ShouldFilterByUserEmail()
    {
        // Arrange
        var userEmail = "applicant@test.com";
        _repositoryMock
            .Setup(r => r.GetDashboardStatsAsync(
                It.Is<EmailAddress>(e => e.Value == userEmail),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((2, 3, 1, 0));

        var query = new GetDashboardStatsQuery(userEmail, "applicant");

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Draft.Should().Be(2);
        result.Value.Submitted.Should().Be(3);
        result.Value.Approved.Should().Be(1);
        result.Value.Rejected.Should().Be(0);
        result.Value.Total.Should().Be(6);

        _repositoryMock.Verify(r => r.GetDashboardStatsAsync(
            It.Is<EmailAddress>(e => e.Value == userEmail),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ProcessorRole_ShouldNotFilterByEmail()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetDashboardStatsAsync(
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((5, 10, 8, 3));

        var query = new GetDashboardStatsQuery("processor@test.com", "processor");

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Draft.Should().Be(5);
        result.Value.Submitted.Should().Be(10);
        result.Value.Approved.Should().Be(8);
        result.Value.Rejected.Should().Be(3);
        result.Value.Total.Should().Be(26);

        _repositoryMock.Verify(r => r.GetDashboardStatsAsync(
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NoApplications_ShouldReturnAllZeros()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetDashboardStatsAsync(
                It.IsAny<EmailAddress?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0, 0, 0));

        var query = new GetDashboardStatsQuery("user@test.com", "applicant");

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(0);
        result.Value.Draft.Should().Be(0);
        result.Value.Submitted.Should().Be(0);
        result.Value.Approved.Should().Be(0);
        result.Value.Rejected.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_TotalEqualsSum_OfAllStatusCounts()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetDashboardStatsAsync(
                It.IsAny<EmailAddress?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((4, 3, 2, 1));

        var query = new GetDashboardStatsQuery("user@test.com", "processor");

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var stats = result.Value!;
        stats.Total.Should().Be(stats.Draft + stats.Submitted + stats.Approved + stats.Rejected);
    }
}
