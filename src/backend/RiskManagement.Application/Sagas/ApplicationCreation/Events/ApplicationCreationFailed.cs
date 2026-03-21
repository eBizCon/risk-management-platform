namespace RiskManagement.Application.Sagas.ApplicationCreation.Events;

public record ApplicationCreationFailed(
    Guid CorrelationId,
    string Reason);