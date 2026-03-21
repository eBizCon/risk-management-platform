namespace SharedKernel.IntegrationEvents;

public record CustomerCreatedIntegrationEvent(int CustomerId, string FirstName, string LastName, string Status);