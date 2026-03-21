namespace RiskManagement.Application.Sagas.ApplicationCreation.Events;

public record CreditCheckCompleted(
    Guid CorrelationId,
    bool HasPaymentDefault,
    int? CreditScore,
    DateTime CheckedAt,
    string Provider);