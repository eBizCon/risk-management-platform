using CustomerManagement.Application.Commands;
using CustomerManagement.Domain.Aggregates.CustomerAggregate;
using CustomerManagement.Domain.Services;
using CustomerManagement.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using SharedKernel.ValueObjects;

namespace RiskManagement.Api.Tests;

public class RequestCreditReportHandlerTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock = new();
    private readonly Mock<ICreditReportProvider> _providerMock = new();
    private readonly RequestCreditReportHandler _handler;

    private const string UserEmail = "admin@test.de";

    public RequestCreditReportHandlerTests()
    {
        _handler = new RequestCreditReportHandler(_repositoryMock.Object, _providerMock.Object);
    }

    private static Customer CreateActiveCustomer()
    {
        return Customer.Create(
            "Max",
            "Mustermann",
            EmailAddress.Create("max@test.de"),
            PhoneNumber.Create("+49123456789"),
            new DateOnly(1990, 1, 1),
            Address.Create("Musterstraße 1", "Berlin", "10115", "Deutschland"),
            EmploymentStatus.Employed,
            EmailAddress.Create(UserEmail));
    }

    [Fact]
    public async Task HandleAsync_ValidCustomer_ShouldReturnSuccessWithCreditReport()
    {
        var customer = CreateActiveCustomer();
        var creditReport = CreditReport.Create(false, 420, DateTime.UtcNow, "schufa_mock");

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<CustomerId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _providerMock.Setup(p => p.CheckAsync(
                customer.FirstName, customer.LastName, customer.DateOfBirth, customer.Address))
            .ReturnsAsync(creditReport);

        var command = new RequestCreditReportCommand(customer.Id.Value, UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Customer.CreditReport.Should().NotBeNull();
        result.Value.Customer.CreditReport!.CreditScore.Should().Be(420);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CustomerNotFound_ShouldReturnNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<CustomerId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new RequestCreditReportCommand(999, UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_WrongUser_ShouldReturnForbidden()
    {
        var customer = CreateActiveCustomer();

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<CustomerId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var command = new RequestCreditReportCommand(customer.Id.Value, "other@test.de");
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task HandleAsync_ArchivedCustomer_ShouldThrowDomainException()
    {
        var customer = CreateActiveCustomer();
        customer.Archive();
        var creditReport = CreditReport.Create(false, 420, DateTime.UtcNow, "schufa_mock");

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<CustomerId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _providerMock.Setup(p => p.CheckAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<Address>()))
            .ReturnsAsync(creditReport);

        var command = new RequestCreditReportCommand(customer.Id.Value, UserEmail);
        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>();
    }
}
