using FluentAssertions;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Exceptions;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Api.Tests;

public class ApplicationDomainPolicyTests
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

    private ApplicationEntity CreateSubmittedApplication(TrafficLight? desiredTrafficLight = null)
    {
        // Use high income for Green, moderate for Yellow, low for Red
        var income = desiredTrafficLight == TrafficLight.Red ? 2500m
            : desiredTrafficLight == TrafficLight.Yellow ? 4000m
            : 6000m;
        var fixedCosts = desiredTrafficLight == TrafficLight.Red ? 2000m
            : desiredTrafficLight == TrafficLight.Yellow ? 2200m
            : 2000m;
        var desiredRate = desiredTrafficLight == TrafficLight.Red ? 400m
            : desiredTrafficLight == TrafficLight.Yellow ? 700m
            : 500m;
        var employmentStatus = desiredTrafficLight == TrafficLight.Red ? EmploymentStatus.Unemployed
            : desiredTrafficLight == TrafficLight.Yellow ? EmploymentStatus.SelfEmployed
            : EmploymentStatus.Employed;
        var hasPaymentDefault = desiredTrafficLight == TrafficLight.Red;
        var creditScore = desiredTrafficLight == TrafficLight.Red ? 150
            : desiredTrafficLight == TrafficLight.Yellow ? 300
            : 450;

        var configVersion = CreateConfigVersion();
        var app = ApplicationEntity.Create(
            1,
            Money.Create(income),
            Money.Create(fixedCosts),
            Money.CreatePositive(desiredRate),
            employmentStatus,
            CreateCreditReport(hasPaymentDefault, creditScore),
            EmailAddress.Create("user@test.com"),
            _scoringService,
            ScoringConfig.Default,
            configVersion.Id);

        app.Submit(_scoringService, ScoringConfig.Default, configVersion.Id);
        return app;
    }

    private ApplicationEntity CreateDraftApplication()
    {
        var configVersion = CreateConfigVersion();
        return ApplicationEntity.Create(
            1,
            Money.Create(6000),
            Money.Create(2000),
            Money.CreatePositive(500),
            EmploymentStatus.Employed,
            CreateCreditReport(),
            EmailAddress.Create("user@test.com"),
            _scoringService,
            ScoringConfig.Default,
            configVersion.Id);
    }

    // ===== Fehler 2: Rescore Guard Clause Tests =====

    [Fact]
    public void Rescore_WhenApproved_ShouldThrowDomainException()
    {
        var app = CreateSubmittedApplication(TrafficLight.Green);
        app.Approve();

        var configVersion = CreateConfigVersion();
        var act = () => app.Rescore(_scoringService, ScoringConfig.Default, configVersion.Id);

        act.Should().Throw<DomainException>()
            .WithMessage("*Abgeschlossene oder in Verarbeitung befindliche*");
    }

    [Fact]
    public void Rescore_WhenRejected_ShouldThrowDomainException()
    {
        var app = CreateSubmittedApplication(TrafficLight.Red);
        app.Reject();

        var configVersion = CreateConfigVersion();
        var act = () => app.Rescore(_scoringService, ScoringConfig.Default, configVersion.Id);

        act.Should().Throw<DomainException>()
            .WithMessage("*Abgeschlossene oder in Verarbeitung befindliche*");
    }

    [Fact]
    public void Rescore_WhenFailed_ShouldThrowDomainException()
    {
        var app = ApplicationEntity.CreateProcessing(
            1, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmailAddress.Create("user@test.com"),
            false);
        app.MarkFailed("some reason");

        var configVersion = CreateConfigVersion();
        var act = () => app.Rescore(_scoringService, ScoringConfig.Default, configVersion.Id);

        act.Should().Throw<DomainException>()
            .WithMessage("*Abgeschlossene oder in Verarbeitung befindliche*");
    }

    [Fact]
    public void Rescore_WhenProcessing_ShouldThrowDomainException()
    {
        var app = ApplicationEntity.CreateProcessing(
            1, Money.Create(5000), Money.Create(2000), Money.CreatePositive(500),
            EmailAddress.Create("user@test.com"),
            false);

        var configVersion = CreateConfigVersion();
        var act = () => app.Rescore(_scoringService, ScoringConfig.Default, configVersion.Id);

        act.Should().Throw<DomainException>()
            .WithMessage("*Abgeschlossene oder in Verarbeitung befindliche*");
    }

    [Fact]
    public void Rescore_WhenDraft_ShouldSucceed()
    {
        var app = CreateDraftApplication();

        var configVersion = CreateConfigVersion();
        var act = () => app.Rescore(_scoringService, ScoringConfig.Default, configVersion.Id);

        act.Should().NotThrow();
    }

    [Fact]
    public void Rescore_WhenSubmitted_ShouldSucceed()
    {
        var app = CreateSubmittedApplication(TrafficLight.Green);

        var configVersion = CreateConfigVersion();
        var act = () => app.Rescore(_scoringService, ScoringConfig.Default, configVersion.Id);

        act.Should().NotThrow();
    }

    // ===== Fehler 3: Approve/Reject Policy Tests =====

    [Fact]
    public void Approve_WhenRedWithoutComment_ShouldThrowDomainException()
    {
        var app = CreateSubmittedApplication(TrafficLight.Red);

        var act = () => app.Approve();

        act.Should().Throw<DomainException>()
            .WithMessage("*Ampelstatus Gelb oder Rot*Begründung*");
    }

    [Fact]
    public void Approve_WhenRedWithComment_ShouldSucceed()
    {
        var app = CreateSubmittedApplication(TrafficLight.Red);

        var act = () => app.Approve("Sondergenehmigung durch Abteilungsleiter");

        act.Should().NotThrow();
        app.Status.Should().Be(ApplicationStatus.Approved);
    }

    [Fact]
    public void Approve_WhenYellowWithoutComment_ShouldThrowDomainException()
    {
        var app = CreateSubmittedApplication(TrafficLight.Yellow);

        var act = () => app.Approve();

        act.Should().Throw<DomainException>()
            .WithMessage("*Ampelstatus Gelb oder Rot*Begründung*");
    }

    [Fact]
    public void Approve_WhenYellowWithComment_ShouldSucceed()
    {
        var app = CreateSubmittedApplication(TrafficLight.Yellow);

        var act = () => app.Approve("Guter Gesamteindruck trotz moderatem Score");

        act.Should().NotThrow();
        app.Status.Should().Be(ApplicationStatus.Approved);
    }

    [Fact]
    public void Approve_WhenGreenWithoutComment_ShouldSucceed()
    {
        var app = CreateSubmittedApplication(TrafficLight.Green);

        var act = () => app.Approve();

        act.Should().NotThrow();
        app.Status.Should().Be(ApplicationStatus.Approved);
    }

    [Fact]
    public void Reject_WhenGreenWithoutComment_ShouldThrowDomainException()
    {
        var app = CreateSubmittedApplication(TrafficLight.Green);

        var act = () => app.Reject();

        act.Should().Throw<DomainException>()
            .WithMessage("*Ampelstatus Grün*Begründung*");
    }

    [Fact]
    public void Reject_WhenGreenWithComment_ShouldSucceed()
    {
        var app = CreateSubmittedApplication(TrafficLight.Green);

        var act = () => app.Reject("Interne Policy-Einschränkung");

        act.Should().NotThrow();
        app.Status.Should().Be(ApplicationStatus.Rejected);
    }

    [Fact]
    public void Reject_WhenRedWithoutComment_ShouldSucceed()
    {
        var app = CreateSubmittedApplication(TrafficLight.Red);

        var act = () => app.Reject();

        act.Should().NotThrow();
        app.Status.Should().Be(ApplicationStatus.Rejected);
    }
}
