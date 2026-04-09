using MassTransit;

namespace RiskManagement.Application.Sagas.ApplicationCreation;

public class ApplicationCreationState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;

    public int ApplicationId { get; set; }
    public int CustomerId { get; set; }
    public double Income { get; set; }
    public double FixedCosts { get; set; }
    public double DesiredRate { get; set; }
    public double LoanAmount { get; set; }
    public int LoanTerm { get; set; }
    public string UserEmail { get; set; } = null!;
    public bool AutoSubmit { get; set; }
    public string OperationType { get; set; } = "Create";

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmploymentStatus { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }

    public bool? HasPaymentDefault { get; set; }
    public int? CreditScore { get; set; }
    public DateTime? CreditCheckedAt { get; set; }
    public string? CreditProvider { get; set; }

    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
