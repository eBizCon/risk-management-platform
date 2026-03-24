using FluentAssertions;
using Moq;
using RiskManagement.Application.Queries;
using RiskManagement.Domain.ReadModels;

namespace RiskManagement.Api.Tests;

public class GetActiveCustomersHandlerTests
{
    private readonly Mock<ICustomerReadModelRepository> _repositoryMock = new();
    private readonly GetActiveCustomersHandler _handler;

    public GetActiveCustomersHandlerTests()
    {
        _handler = new GetActiveCustomersHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithActiveCustomers_ShouldReturnDtos()
    {
        var customers = new List<CustomerReadModel>
        {
            new()
            {
                CustomerId = 1, FirstName = "Anna", LastName = "Schmidt", Status = "active",
                LastUpdatedAt = DateTime.UtcNow
            },
            new()
            {
                CustomerId = 2, FirstName = "Max", LastName = "Mustermann", Status = "active",
                LastUpdatedAt = DateTime.UtcNow
            }
        };
        _repositoryMock.Setup(r => r.GetActiveCustomersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        var result = await _handler.HandleAsync(new GetActiveCustomersQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value![0].CustomerId.Should().Be(1);
        result.Value[0].FirstName.Should().Be("Anna");
        result.Value[1].CustomerId.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_WithNoCustomers_ShouldReturnEmptyList()
    {
        _repositoryMock.Setup(r => r.GetActiveCustomersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CustomerReadModel>());

        var result = await _handler.HandleAsync(new GetActiveCustomersQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}