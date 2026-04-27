using FluentAssertions;
using Moq;
using RiskManagement.Application.Queries;
using RiskManagement.Domain.ReadModels;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Api.Tests;

public class GetDashboardStatsHandlerTests
{
    private readonly Mock<IDashboardStatsQuery> _dashboardStatsMock = new();
    private readonly GetDashboardStatsHandler _handler;

    public GetDashboardStatsHandlerTests()
    {
        _handler = new GetDashboardStatsHandler(_dashboardStatsMock.Object);
    }

    [Fact]
    public async Task HandleAsync_AsApplicant_ShouldFilterByUserEmail()
    {
        _dashboardStatsMock.Setup(r => r.GetStatsAsync(
                It.Is<EmailAddress>(e => e == EmailAddress.Create("applicant@example.com")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((15, 4, 3, 3, 2));

        var result = await _handler.HandleAsync(
            new GetDashboardStatsQuery("applicant@example.com", "applicant"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(15);
        result.Value.Draft.Should().Be(4);
        result.Value.Submitted.Should().Be(3);
        result.Value.Approved.Should().Be(3);
        result.Value.Rejected.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_AsProcessor_ShouldNotFilterByUser()
    {
        _dashboardStatsMock.Setup(r => r.GetStatsAsync(
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((30, 10, 5, 8, 3));

        var result = await _handler.HandleAsync(
            new GetDashboardStatsQuery("processor@example.com", "processor"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(30);
        result.Value.Draft.Should().Be(10);
        result.Value.Submitted.Should().Be(5);
        result.Value.Approved.Should().Be(8);
        result.Value.Rejected.Should().Be(3);
    }

    [Fact]
    public async Task HandleAsync_WithZeroCounts_ShouldReturnZeroTotal()
    {
        _dashboardStatsMock.Setup(r => r.GetStatsAsync(
                It.IsAny<EmailAddress?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0, 0, 0, 0));

        var result = await _handler.HandleAsync(
            new GetDashboardStatsQuery("user@example.com", "applicant"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(0);
        result.Value.Draft.Should().Be(0);
        result.Value.Submitted.Should().Be(0);
        result.Value.Approved.Should().Be(0);
        result.Value.Rejected.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_TotalIncludesAllStatuses_NotJustDisplayedOnes()
    {
        _dashboardStatsMock.Setup(r => r.GetStatsAsync(
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((50, 1, 2, 3, 4));

        var result = await _handler.HandleAsync(
            new GetDashboardStatsQuery("proc@example.com", "processor"));

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value!;
        dto.Total.Should().Be(50);
        dto.Total.Should().BeGreaterThanOrEqualTo(dto.Draft + dto.Submitted + dto.Approved + dto.Rejected);
    }
}
