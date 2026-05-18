using FluentAssertions;
using Moq;
using RiskManagement.Application.DTOs;
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
    public async Task HandleAsync_Applicant_ShouldFilterByEmail()
    {
        _repositoryMock
            .Setup(r => r.GetDashboardStatsAsync(It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((2, 1, 3, 1));

        var query = new GetDashboardStatsQuery("applicant@test.com", "applicant");
        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Draft.Should().Be(2);
        result.Value.Submitted.Should().Be(1);
        result.Value.Approved.Should().Be(3);
        result.Value.Rejected.Should().Be(1);
        result.Value.Total.Should().Be(7);

        _repositoryMock.Verify(
            r => r.GetDashboardStatsAsync(It.Is<EmailAddress>(e => e.Value == "applicant@test.com"),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Processor_ShouldNotFilterByEmail()
    {
        _repositoryMock
            .Setup(r => r.GetDashboardStatsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((5, 3, 4, 2));

        var query = new GetDashboardStatsQuery("processor@test.com", "processor");
        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Draft.Should().Be(5);
        result.Value.Submitted.Should().Be(3);
        result.Value.Approved.Should().Be(4);
        result.Value.Rejected.Should().Be(2);
        result.Value.Total.Should().Be(14);

        _repositoryMock.Verify(
            r => r.GetDashboardStatsAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AllZero_ShouldReturnZeroTotal()
    {
        _repositoryMock
            .Setup(r => r.GetDashboardStatsAsync(It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0, 0, 0));

        var query = new GetDashboardStatsQuery("applicant@test.com", "applicant");
        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(0);
        result.Value.Draft.Should().Be(0);
        result.Value.Submitted.Should().Be(0);
        result.Value.Approved.Should().Be(0);
        result.Value.Rejected.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_TotalIsOnlySumOfFourStatuses()
    {
        _repositoryMock
            .Setup(r => r.GetDashboardStatsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((4, 3, 3, 2));

        var query = new GetDashboardStatsQuery("processor@test.com", "processor");
        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(12);
    }
}
