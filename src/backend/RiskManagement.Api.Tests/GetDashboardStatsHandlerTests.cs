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
    public async Task HandleAsync_AsApplicant_ShouldFilterByUserEmail()
    {
        _repositoryMock.Setup(r => r.GetDashboardStatsAsync(
                It.Is<EmailAddress>(e => e == EmailAddress.Create("applicant@example.com")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((4, 3, 3, 2));

        var result = await _handler.HandleAsync(
            new GetDashboardStatsQuery("applicant@example.com", "applicant"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Draft.Should().Be(4);
        result.Value.Submitted.Should().Be(3);
        result.Value.Approved.Should().Be(3);
        result.Value.Rejected.Should().Be(2);
        result.Value.Total.Should().Be(12);
    }

    [Fact]
    public async Task HandleAsync_AsProcessor_ShouldNotFilterByUser()
    {
        _repositoryMock.Setup(r => r.GetDashboardStatsAsync(
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((10, 5, 8, 3));

        var result = await _handler.HandleAsync(
            new GetDashboardStatsQuery("processor@example.com", "processor"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Draft.Should().Be(10);
        result.Value.Submitted.Should().Be(5);
        result.Value.Approved.Should().Be(8);
        result.Value.Rejected.Should().Be(3);
        result.Value.Total.Should().Be(26);
    }

    [Fact]
    public async Task HandleAsync_WithZeroCounts_ShouldReturnZeroTotal()
    {
        _repositoryMock.Setup(r => r.GetDashboardStatsAsync(
                It.IsAny<EmailAddress?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0, 0, 0));

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
    public async Task HandleAsync_TotalEqualsSum_OfAllStatusCounts()
    {
        _repositoryMock.Setup(r => r.GetDashboardStatsAsync(
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((1, 2, 3, 4));

        var result = await _handler.HandleAsync(
            new GetDashboardStatsQuery("proc@example.com", "processor"));

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value!;
        dto.Total.Should().Be(dto.Draft + dto.Submitted + dto.Approved + dto.Rejected);
    }
}
