namespace SharedKernel.IntegrationEvents;

public record CustomerUpdatedIntegrationEvent(int CustomerId, string FirstName, string LastName, string Status);