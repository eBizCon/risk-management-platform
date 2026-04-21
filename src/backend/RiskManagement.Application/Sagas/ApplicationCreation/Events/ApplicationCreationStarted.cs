namespace RiskManagement.Application.Sagas.ApplicationCreation.Events;

public record ApplicationCreationStarted(
    Guid CorrelationId,
    int ApplicationId,
    int CustomerId,
    double Income,
    double FixedCosts,
    double DesiredRate,
    double LoanAmount,
    int LoanTerm,
    string UserEmail,
    bool AutoSubmit);
