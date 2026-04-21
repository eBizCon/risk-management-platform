using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using FluentValidationResult = FluentValidation.Results.ValidationResult;
using MassTransit;
using Moq;
using RiskManagement.Application.Commands;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Api.Tests;

public class UpdateAndSubmitApplicationHandlerTests
{
    private readonly Mock<IApplicationRepository> _repositoryMock = new();
    private readonly Mock<IValidator<ApplicationUpdateDto>> _validatorMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly UpdateAndSubmitApplicationHandler _handler;

    private const string UserEmail = "user@test.com";

    public UpdateAndSubmitApplicationHandlerTests()
    {
        _handler = new UpdateAndSubmitApplicationHandler(
            _repositoryMock.Object,
            _validatorMock.Object,
            _publishEndpointMock.Object);

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ApplicationUpdateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidationResult());
    }

    private static ApplicationEntity CreateDraftApp()
    {
        var scoringService = new ScoringService();
        var config = ScoringConfig.Default;
        var configVersion = ScoringConfigVersion.Create(1, config, EmailAddress.Create("admin@test.com"));

        return ApplicationEntity.Create(
            1,
            Money.Create(5000),
            Money.Create(2000),
            Money.CreatePositive(500),
            Money.CreatePositive(25000),
            48,
            EmploymentStatus.Employed,
            CreditReport.Create(false, 420, DateTime.UtcNow, "schufa_mock"),
            EmailAddress.Create(UserEmail),
            scoringService,
            config,
            configVersion.Id);
    }

    private static ApplicationUpdateDto CreateValidDto()
    {
        return new ApplicationUpdateDto
        {
            CustomerId = 1,
            Income = 6000,
            FixedCosts = 2500,
            DesiredRate = 600,
            LoanAmount = 30000,
            LoanTerm = 60
        };
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ShouldSetProcessingAndPublishWithAutoSubmitTrue()
    {
        var app = CreateDraftApp();
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<AppId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(app);

        var command = new UpdateAndSubmitApplicationCommand(1, CreateValidDto(), UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Application.Status.Should().Be("processing");
        app.Status.Should().Be(ApplicationStatus.Processing);

        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(p => p.Publish(
            It.Is<ApplicationUpdateStarted>(e =>
                e.CustomerId == 1 &&
                e.Income == 6000 &&
                e.AutoSubmit == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ApplicationNotFound_ShouldReturnNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<AppId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationEntity?)null);

        var command = new UpdateAndSubmitApplicationCommand(1, CreateValidDto(), UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<ApplicationUpdateStarted>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidationFails_ShouldReturnValidationFailure()
    {
        var app = CreateDraftApp();
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<AppId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(app);

        var failures = new List<ValidationFailure> { new("Income", "Einkommen muss positiv sein") };
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ApplicationUpdateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidationResult(failures));

        var command = new UpdateAndSubmitApplicationCommand(1, CreateValidDto(), UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<ApplicationUpdateStarted>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
