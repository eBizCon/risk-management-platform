using FluentAssertions;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Api.Tests;

public class ApplicationSnapshotTests
{
    private readonly ScoringService _scoringService = new();

    private static ScoringConfigVersion CreateConfigVersion()
    {
        return ScoringConfigVersion.Create(1, ScoringConfig.Default, EmailAddress.Create("admin@test.com"));
    }

    private static CreditReport CreateCreditReport(bool hasPaymentDefault = false, int? creditScore = 420)
    {
        return CreditReport.Create(hasPaymentDefault, creditScore, DateTime.UtcNow, "schufa_mock");
    }

    [Fact]
    public void Create_WithSnapshotValues_ShouldStoreEmploymentStatus()
    {
        var configVersion = CreateConfigVersion();

        var app = ApplicationEntity.Create(
            1, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmploymentStatus.SelfEmployed, CreateCreditReport(),
            EmailAddress.Create("user@test.com"),
            _scoringService, ScoringConfig.Default, configVersion.Id);

        app.EmploymentStatus.Should().Be(EmploymentStatus.SelfEmployed);
        app.CreditReport.HasPaymentDefault.Should().BeFalse();
        app.CreditReport.CreditScore.Should().Be(420);
    }

    [Fact]
    public void Create_WithPaymentDefault_ShouldStoreSnapshot()
    {
        var configVersion = CreateConfigVersion();

        var app = ApplicationEntity.Create(
            1, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmploymentStatus.Unemployed, CreateCreditReport(true, 250),
            EmailAddress.Create("user@test.com"),
            _scoringService, ScoringConfig.Default, configVersion.Id);

        app.CreditReport.HasPaymentDefault.Should().BeTrue();
        app.CreditReport.CreditScore.Should().Be(250);
        app.EmploymentStatus.Should().Be(EmploymentStatus.Unemployed);
    }

    [Fact]
    public void Create_WithNullCreditScore_ShouldStoreNull()
    {
        var configVersion = CreateConfigVersion();

        var app = ApplicationEntity.Create(
            1, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmploymentStatus.Employed, CreateCreditReport(creditScore: null),
            EmailAddress.Create("user@test.com"),
            _scoringService, ScoringConfig.Default, configVersion.Id);

        app.CreditReport.CreditScore.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldApplyScoring()
    {
        var configVersion = CreateConfigVersion();

        var app = ApplicationEntity.Create(
            1, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmploymentStatus.Employed, CreateCreditReport(),
            EmailAddress.Create("user@test.com"),
            _scoringService, ScoringConfig.Default, configVersion.Id);

        app.Score.Should().NotBeNull();
        app.TrafficLight.Should().NotBeNull();
        app.ScoringConfigVersionId.Should().Be(configVersion.Id);
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateSnapshotValues()
    {
        var configVersion = CreateConfigVersion();

        var app = ApplicationEntity.Create(
            1, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmploymentStatus.Employed, CreateCreditReport(),
            EmailAddress.Create("user@test.com"),
            _scoringService, ScoringConfig.Default, configVersion.Id);

        app.UpdateDetails(
            2, Money.Create(6000), Money.Create(2500), Money.CreatePositive(600),
            EmploymentStatus.Retired, CreateCreditReport(true, 300),
            _scoringService, ScoringConfig.Default, configVersion.Id);

        app.CustomerId.Should().Be(2);
        app.EmploymentStatus.Should().Be(EmploymentStatus.Retired);
        app.CreditReport.HasPaymentDefault.Should().BeTrue();
        app.CreditReport.CreditScore.Should().Be(300);
        app.Income.Should().Be(Money.Create(6000));
    }

    [Fact]
    public void UpdateDetails_NonDraftStatus_ShouldThrowDomainException()
    {
        var configVersion = CreateConfigVersion();

        var app = ApplicationEntity.Create(
            1, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmploymentStatus.Employed, CreateCreditReport(),
            EmailAddress.Create("user@test.com"),
            _scoringService, ScoringConfig.Default, configVersion.Id);
        app.Submit(_scoringService, ScoringConfig.Default, configVersion.Id);

        var act = () => app.UpdateDetails(
            1, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmploymentStatus.Employed, CreateCreditReport(),
            _scoringService, ScoringConfig.Default, configVersion.Id);

        act.Should().Throw<DomainException>().WithMessage("*Entwürfe*");
    }

    [Fact]
    public void UpdateDetails_ShouldRescoreAfterUpdate()
    {
        var configVersion = CreateConfigVersion();

        var app = ApplicationEntity.Create(
            1, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmploymentStatus.Employed, CreateCreditReport(),
            EmailAddress.Create("user@test.com"),
            _scoringService, ScoringConfig.Default, configVersion.Id);

        var scoreBefore = app.Score;

        app.UpdateDetails(
            1, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmploymentStatus.Unemployed, CreateCreditReport(true, 200),
            _scoringService, ScoringConfig.Default, configVersion.Id);

        app.Score.Should().NotBe(scoreBefore);
    }
}