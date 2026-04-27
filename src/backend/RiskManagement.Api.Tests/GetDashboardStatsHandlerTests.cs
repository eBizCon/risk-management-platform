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
    public async Task HandleAsync_ProcessorRole_ShouldReturnGlobalStats()
    {
        _repositoryMock
            .Setup(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((4, 3, 3, 2));

        var query = new GetDashboardStatsQuery("processor@test.com", "processor");
        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Draft.Should().Be(4);
        result.Value.Submitted.Should().Be(3);
        result.Value.Approved.Should().Be(3);
        result.Value.Rejected.Should().Be(2);
        result.Value.Total.Should().Be(12);

        _repositoryMock.Verify(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(
            r => r.GetDashboardStatsByUserAsync(It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ApplicantRole_ShouldReturnUserFilteredStats()
    {
        _repositoryMock
            .Setup(r => r.GetDashboardStatsByUserAsync(It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((2, 1, 1, 0));

        var query = new GetDashboardStatsQuery("applicant@test.com", "applicant");
        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Draft.Should().Be(2);
        result.Value.Submitted.Should().Be(1);
        result.Value.Approved.Should().Be(1);
        result.Value.Rejected.Should().Be(0);
        result.Value.Total.Should().Be(4);

        _repositoryMock.Verify(
            r => r.GetDashboardStatsByUserAsync(It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AllZeros_ShouldReturnZeroTotal()
    {
        _repositoryMock
            .Setup(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0, 0, 0));

        var query = new GetDashboardStatsQuery("processor@test.com", "processor");
        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(0);
        result.Value.Draft.Should().Be(0);
        result.Value.Submitted.Should().Be(0);
        result.Value.Approved.Should().Be(0);
        result.Value.Rejected.Should().Be(0);
    }
}
