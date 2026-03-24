namespace RiskManagement.Application.Sagas.ApplicationCreation.Events;

public record PerformCreditCheck(
    Guid CorrelationId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Street,
    string City,
    string ZipCode,
    string Country);