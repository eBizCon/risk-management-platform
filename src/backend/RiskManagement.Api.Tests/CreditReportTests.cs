using FluentAssertions;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Api.Tests;

public class CreditReportTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnCreditReport()
    {
        var checkedAt = DateTime.UtcNow;

        var report = CreditReport.Create(false, 420, checkedAt, "schufa_mock");

        report.HasPaymentDefault.Should().BeFalse();
        report.CreditScore.Should().Be(420);
        report.CheckedAt.Should().Be(checkedAt);
        report.Provider.Should().Be("schufa_mock");
    }

    [Fact]
    public void Create_WithPaymentDefault_ShouldReturnCreditReport()
    {
        var report = CreditReport.Create(true, 250, DateTime.UtcNow, "schufa_mock");

        report.HasPaymentDefault.Should().BeTrue();
        report.CreditScore.Should().Be(250);
    }

    [Fact]
    public void Create_WithNullCreditScore_ShouldReturnCreditReport()
    {
        var report = CreditReport.Create(false, null, DateTime.UtcNow, "schufa_mock");

        report.CreditScore.Should().BeNull();
    }

    [Fact]
    public void Create_WithMinBoundaryScore_ShouldReturnCreditReport()
    {
        var report = CreditReport.Create(false, 100, DateTime.UtcNow, "schufa_mock");

        report.CreditScore.Should().Be(100);
    }

    [Fact]
    public void Create_WithMaxBoundaryScore_ShouldReturnCreditReport()
    {
        var report = CreditReport.Create(false, 600, DateTime.UtcNow, "schufa_mock");

        report.CreditScore.Should().Be(600);
    }

    [Theory]
    [InlineData(99)]
    [InlineData(601)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1000)]
    public void Create_WithInvalidScore_ShouldThrowDomainException(int invalidScore)
    {
        var act = () => CreditReport.Create(false, invalidScore, DateTime.UtcNow, "schufa_mock");

        act.Should().Throw<DomainException>().WithMessage("*CreditScore*100*600*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyProvider_ShouldThrowDomainException(string? provider)
    {
        var act = () => CreditReport.Create(false, 420, DateTime.UtcNow, provider!);

        act.Should().Throw<DomainException>().WithMessage("*Provider*leer*");
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var checkedAt = DateTime.UtcNow;
        var a = CreditReport.Create(false, 420, checkedAt, "schufa_mock");
        var b = CreditReport.Create(false, 420, checkedAt, "schufa_mock");

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        var checkedAt = DateTime.UtcNow;
        var a = CreditReport.Create(false, 420, checkedAt, "schufa_mock");
        var b = CreditReport.Create(true, 250, checkedAt, "schufa_mock");

        a.Should().NotBe(b);
    }
}
