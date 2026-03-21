namespace RiskManagement.Application.Sagas.ApplicationCreation.Events;

public record FetchCustomerProfile(
    Guid CorrelationId,
    int CustomerId);