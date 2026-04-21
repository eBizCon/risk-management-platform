namespace RiskManagement.Application.Sagas.ApplicationCreation.Events;

public record FinalizeApplicationUpdate(
    Guid CorrelationId,
    int ApplicationId,
    int CustomerId,
    double Income,
    double FixedCosts,
    double DesiredRate,
    double LoanAmount,
    int LoanTerm,
    string UserEmail,
    string EmploymentStatus,
    bool HasPaymentDefault,
    int? CreditScore,
    DateTime CreditCheckedAt,
    string CreditProvider,
    bool AutoSubmit);
