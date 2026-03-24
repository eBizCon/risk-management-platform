namespace RiskManagement.Application.Sagas.ApplicationCreation.Events;

public record CustomerProfileFetched(
    Guid CorrelationId,
    string FirstName,
    string LastName,
    string EmploymentStatus,
    string DateOfBirth,
    string Street,
    string City,
    string ZipCode,
    string Country);