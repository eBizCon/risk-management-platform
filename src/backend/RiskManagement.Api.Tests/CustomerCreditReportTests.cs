using CustomerManagement.Domain.Aggregates.CustomerAggregate;
using CustomerManagement.Domain.Events;
using CustomerManagement.Domain.ValueObjects;
using FluentAssertions;
using SharedKernel.ValueObjects;

namespace RiskManagement.Api.Tests;

public class CustomerCreditReportTests
{
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
            EmailAddress.Create("admin@test.de"));
    }

    [Fact]
    public void UpdateCreditReport_ActiveCustomer_ShouldSetCreditReport()
    {
        var customer = CreateActiveCustomer();
        var report = CreditReport.Create(false, 420, DateTime.UtcNow, "schufa_mock");

        customer.UpdateCreditReport(report);

        customer.CreditReport.Should().Be(report);
        customer.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateCreditReport_ActiveCustomer_ShouldPublishEvent()
    {
        var customer = CreateActiveCustomer();
        var report = CreditReport.Create(true, 250, DateTime.UtcNow, "schufa_mock");

        customer.UpdateCreditReport(report);

        var evt = customer.DomainEvents.OfType<CreditReportReceivedEvent>().Should().ContainSingle().Subject;
        evt.HasPaymentDefault.Should().BeTrue();
        evt.CreditScore.Should().Be(250);
    }

    [Fact]
    public void UpdateCreditReport_ArchivedCustomer_ShouldThrowDomainException()
    {
        var customer = CreateActiveCustomer();
        customer.Archive();
        var report = CreditReport.Create(false, 420, DateTime.UtcNow, "schufa_mock");

        var act = () => customer.UpdateCreditReport(report);

        act.Should().Throw<DomainException>().WithMessage("*aktiv*");
    }

    [Fact]
    public void UpdateCreditReport_CalledTwice_ShouldReplaceReport()
    {
        var customer = CreateActiveCustomer();
        var firstReport = CreditReport.Create(false, 420, DateTime.UtcNow, "schufa_mock");
        var secondReport = CreditReport.Create(true, 250, DateTime.UtcNow, "schufa_mock");

        customer.UpdateCreditReport(firstReport);
        customer.UpdateCreditReport(secondReport);

        customer.CreditReport.Should().Be(secondReport);
    }
}
