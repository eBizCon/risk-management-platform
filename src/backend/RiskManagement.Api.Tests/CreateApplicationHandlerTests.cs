using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using RiskManagement.Application.Commands;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Services;
using RiskManagement.Application.Validation;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Api.Tests;

public class CreateApplicationHandlerTests
{
    private readonly Mock<IApplicationRepository> _repositoryMock = new();
    private readonly Mock<IScoringConfigRepository> _configRepoMock = new();
    private readonly Mock<ICustomerProfileService> _profileServiceMock = new();
    private readonly Mock<IValidator<ApplicationCreateDto>> _validatorMock = new();
    private readonly ScoringService _scoringService = new();
    private readonly CreateApplicationHandler _handler;

    private const string UserEmail = "user@test.com";

    public CreateApplicationHandlerTests()
    {
        _handler = new CreateApplicationHandler(
            _repositoryMock.Object,
            _configRepoMock.Object,
            _scoringService,
            _validatorMock.Object,
            _profileServiceMock.Object);

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ApplicationCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private void SetupValidCustomerProfile()
    {
        _profileServiceMock.Setup(p => p.GetCustomerProfileAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerProfile(
                1, "Max", "Mustermann", "employed",
                new CustomerCreditReport(false, 420, DateTime.UtcNow.ToString("o"), "schufa_mock"),
                "Active"));
    }

    private void SetupScoringConfig()
    {
        var configVersion = ScoringConfigVersion.Create(1, ScoringConfig.Default, EmailAddress.Create("admin@test.com"));
        _configRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(configVersion);
    }

    private static ApplicationCreateDto CreateValidDto() => new()
    {
        CustomerId = 1,
        Income = 5000,
        FixedCosts = 2000,
        DesiredRate = 500
    };

    [Fact]
    public async Task HandleAsync_ValidRequest_ShouldReturnSuccess()
    {
        SetupValidCustomerProfile();
        SetupScoringConfig();

        var command = new CreateApplicationCommand(CreateValidDto(), UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Application.Should().NotBeNull();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ApplicationEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CustomerNotFound_ShouldReturnFailure()
    {
        SetupScoringConfig();
        _profileServiceMock.Setup(p => p.GetCustomerProfileAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerProfile?)null);

        var command = new CreateApplicationCommand(CreateValidDto(), UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Kunde nicht gefunden");
    }

    [Fact]
    public async Task HandleAsync_NoCreditReport_ShouldReturnFailure()
    {
        SetupScoringConfig();
        _profileServiceMock.Setup(p => p.GetCustomerProfileAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerProfile(
                1, "Max", "Mustermann", "employed",
                null,
                "Active"));

        var command = new CreateApplicationCommand(CreateValidDto(), UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Bonitätsprüfung");
    }

    [Fact]
    public async Task HandleAsync_NoScoringConfig_ShouldReturnFailure()
    {
        SetupValidCustomerProfile();
        _configRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScoringConfigVersion?)null);

        var command = new CreateApplicationCommand(CreateValidDto(), UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Scoring-Konfiguration");
    }

    [Fact]
    public async Task HandleAsync_ValidationFails_ShouldReturnValidationFailure()
    {
        var failures = new List<ValidationFailure> { new("Income", "Einkommen muss positiv sein") };
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ApplicationCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var command = new CreateApplicationCommand(CreateValidDto(), UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error!.ValidationErrors.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ShouldPassSnapshotValuesToApplication()
    {
        _profileServiceMock.Setup(p => p.GetCustomerProfileAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerProfile(
                1, "Max", "Mustermann", "self_employed",
                new CustomerCreditReport(true, 250, DateTime.UtcNow.ToString("o"), "schufa_mock"),
                "Active"));
        SetupScoringConfig();

        ApplicationEntity? capturedApp = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<ApplicationEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ApplicationEntity, CancellationToken>((app, _) => capturedApp = app)
            .Returns(Task.CompletedTask);

        var command = new CreateApplicationCommand(CreateValidDto(), UserEmail);
        await _handler.HandleAsync(command);

        capturedApp.Should().NotBeNull();
        capturedApp!.EmploymentStatus.Should().Be(EmploymentStatus.SelfEmployed);
        capturedApp.HasPaymentDefault.Should().BeTrue();
        capturedApp.CreditScore.Should().Be(250);
    }
}
