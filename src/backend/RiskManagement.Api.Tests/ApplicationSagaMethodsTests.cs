using FluentAssertions;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Exceptions;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Api.Tests;

public class ApplicationSagaMethodsTests
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

    private static ApplicationEntity CreateProcessingApplication()
    {
        return ApplicationEntity.CreateProcessing(
            1,
            Money.Create(5000),
            Money.Create(2000),
            Money.CreatePositive(500),
            EmailAddress.Create("user@test.com"),
            false);
    }

    [Fact]
    public void CreateProcessing_ValidInput_ShouldSetStatusToProcessing()
    {
        var app = CreateProcessingApplication();

        app.Status.Should().Be(ApplicationStatus.Processing);
    }

    [Fact]
    public void CreateProcessing_ValidInput_ShouldHaveNullCreditReport()
    {
        var app = CreateProcessingApplication();

        app.CreditReport.Should().BeNull();
    }

    [Fact]
    public void CreateProcessing_ValidInput_ShouldHaveNullScore()
    {
        var app = CreateProcessingApplication();

        app.Score.Should().BeNull();
        app.TrafficLight.Should().BeNull();
        app.ScoringReasons.Should().BeNull();
    }

    [Fact]
    public void CreateProcessing_ValidInput_ShouldStoreFinancialData()
    {
        var app = CreateProcessingApplication();

        app.CustomerId.Should().Be(1);
        app.Income.Should().Be(Money.Create(5000));
        app.FixedCosts.Should().Be(Money.Create(2000));
        app.DesiredRate.Should().Be(Money.CreatePositive(500));
    }

    [Fact]
    public void CreateProcessing_InvalidCustomerId_ShouldThrow()
    {
        var act = () => ApplicationEntity.CreateProcessing(
            0, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmailAddress.Create("user@test.com"),
            false);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Finalize_ProcessingApplication_ShouldTransitionToDraft()
    {
        var app = CreateProcessingApplication();
        var configVersion = CreateConfigVersion();

        app.Finalize(
            EmploymentStatus.SelfEmployed,
            CreateCreditReport(true, 250),
            _scoringService,
            ScoringConfig.Default,
            configVersion.Id);

        app.Status.Should().Be(ApplicationStatus.Draft);
    }

    [Fact]
    public void Finalize_ProcessingApplication_ShouldSetEmploymentStatusAndCreditReport()
    {
        var app = CreateProcessingApplication();
        var configVersion = CreateConfigVersion();
        var creditReport = CreateCreditReport(true, 250);

        app.Finalize(
            EmploymentStatus.SelfEmployed,
            creditReport,
            _scoringService,
            ScoringConfig.Default,
            configVersion.Id);

        app.EmploymentStatus.Should().Be(EmploymentStatus.SelfEmployed);
        app.CreditReport.HasPaymentDefault.Should().BeTrue();
        app.CreditReport.CreditScore.Should().Be(250);
    }

    [Fact]
    public void Finalize_ProcessingApplication_ShouldApplyScoring()
    {
        var app = CreateProcessingApplication();
        var configVersion = CreateConfigVersion();

        app.Finalize(
            EmploymentStatus.Employed,
            CreateCreditReport(),
            _scoringService,
            ScoringConfig.Default,
            configVersion.Id);

        app.Score.Should().NotBeNull();
        app.TrafficLight.Should().NotBeNull();
        app.ScoringReasons.Should().NotBeNull();
    }

    [Fact]
    public void Finalize_NonProcessingApplication_ShouldThrow()
    {
        var configVersion = CreateConfigVersion();
        var app = ApplicationEntity.Create(
            1, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmploymentStatus.Employed, CreateCreditReport(),
            EmailAddress.Create("user@test.com"),
            _scoringService, ScoringConfig.Default, configVersion.Id);

        var act = () => app.Finalize(
            EmploymentStatus.Employed,
            CreateCreditReport(),
            _scoringService,
            ScoringConfig.Default,
            configVersion.Id);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkFailed_ProcessingApplication_ShouldTransitionToFailed()
    {
        var app = CreateProcessingApplication();

        app.MarkFailed("Customer not found");

        app.Status.Should().Be(ApplicationStatus.Failed);
        app.FailureReason.Should().Be("Customer not found");
    }

    [Fact]
    public void MarkFailed_NonProcessingApplication_ShouldThrow()
    {
        var configVersion = CreateConfigVersion();
        var app = ApplicationEntity.Create(
            1, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmploymentStatus.Employed, CreateCreditReport(),
            EmailAddress.Create("user@test.com"),
            _scoringService, ScoringConfig.Default, configVersion.Id);

        var act = () => app.MarkFailed("some reason");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Submit_ProcessingApplication_ShouldThrow()
    {
        var app = CreateProcessingApplication();
        var configVersion = CreateConfigVersion();

        var act = () => app.Submit(_scoringService, ScoringConfig.Default, configVersion.Id);

        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void Delete_FailedApplication_ShouldSucceed()
    {
        var app = CreateProcessingApplication();
        app.MarkFailed("some reason");

        var act = () => app.Delete();

        act.Should().NotThrow();
    }

    [Fact]
    public void Delete_ProcessingApplication_ShouldThrow()
    {
        var app = CreateProcessingApplication();

        var act = () => app.Delete();

        act.Should().Throw<DomainException>();
    }
}
