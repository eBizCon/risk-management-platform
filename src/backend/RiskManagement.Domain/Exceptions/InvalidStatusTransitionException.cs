using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Domain.Exceptions;

public class InvalidStatusTransitionException : DomainException
{
    public ApplicationStatus FromStatus { get; }
    public ApplicationStatus ToStatus { get; }

    public InvalidStatusTransitionException(ApplicationStatus fromStatus, ApplicationStatus toStatus)
        : base($"Invalid status transition from '{fromStatus.Value}' to '{toStatus.Value}'")
    {
        FromStatus = fromStatus;
        ToStatus = toStatus;
    }
}