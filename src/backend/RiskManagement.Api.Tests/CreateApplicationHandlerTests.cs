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
using RiskManagement.Domain.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Api.Tests;

public class CreateApplicationHandlerTests
{
    private readonly Mock<IApplicationRepository> _repositoryMock = new();
    private readonly Mock<IValidator<ApplicationCreateDto>> _validatorMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly CreateApplicationHandler _handler;

    private const string UserEmail = "user@test.com";

    public CreateApplicationHandlerTests()
    {
        _handler = new CreateApplicationHandler(
            _repositoryMock.Object,
            _validatorMock.Object,
            _publishEndpointMock.Object);

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ApplicationCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidationResult());
    }

    private static ApplicationCreateDto CreateValidDto()
    {
        return new ApplicationCreateDto
        {
            CustomerId = 1,
            Income = 5000,
            FixedCosts = 2000,
            DesiredRate = 500
        };
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ShouldCreateProcessingApplicationAndPublishSagaEvent()
    {
        ApplicationEntity? capturedApp = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<ApplicationEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ApplicationEntity, CancellationToken>((app, _) => capturedApp = app)
            .Returns(Task.CompletedTask);

        var command = new CreateApplicationCommand(CreateValidDto(), UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Application.Should().NotBeNull();
        result.Value.Application.Status.Should().Be("processing");

        capturedApp.Should().NotBeNull();
        capturedApp!.Status.Should().Be(ApplicationStatus.Processing);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ApplicationEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(p => p.Publish(
            It.Is<ApplicationCreationStarted>(e =>
                e.CustomerId == 1 &&
                e.AutoSubmit == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ValidationFails_ShouldReturnValidationFailure()
    {
        var failures = new List<ValidationFailure> { new("Income", "Einkommen muss positiv sein") };
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ApplicationCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidationResult(failures));

        var command = new CreateApplicationCommand(CreateValidDto(), UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error!.ValidationErrors.Should().NotBeNull();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ApplicationEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<ApplicationCreationStarted>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ShouldSetAutoSubmitFalse()
    {
        var command = new CreateApplicationCommand(CreateValidDto(), UserEmail);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        _publishEndpointMock.Verify(p => p.Publish(
            It.Is<ApplicationCreationStarted>(e => e.AutoSubmit == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}