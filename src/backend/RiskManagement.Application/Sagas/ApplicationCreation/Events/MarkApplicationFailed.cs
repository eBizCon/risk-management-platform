namespace RiskManagement.Application.Sagas.ApplicationCreation.Events;

public record MarkApplicationFailed(
    Guid CorrelationId,
    int ApplicationId,
    string Reason);
