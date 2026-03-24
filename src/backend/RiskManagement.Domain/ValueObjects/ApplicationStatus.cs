using RiskManagement.Domain.Common;

namespace RiskManagement.Domain.ValueObjects;

public sealed class ApplicationStatus : Enumeration<ApplicationStatus>
{
    public static readonly ApplicationStatus Processing = new("processing");
    public static readonly ApplicationStatus Draft = new("draft");
    public static readonly ApplicationStatus Submitted = new("submitted");
    public static readonly ApplicationStatus NeedsInformation = new("needs_information");
    public static readonly ApplicationStatus Resubmitted = new("resubmitted");
    public static readonly ApplicationStatus Approved = new("approved");
    public static readonly ApplicationStatus Rejected = new("rejected");
    public static readonly ApplicationStatus Failed = new("failed");

    private static readonly Dictionary<string, ApplicationStatus> All = new()
    {
        [Processing.Value] = Processing,
        [Draft.Value] = Draft,
        [Submitted.Value] = Submitted,
        [NeedsInformation.Value] = NeedsInformation,
        [Resubmitted.Value] = Resubmitted,
        [Approved.Value] = Approved,
        [Rejected.Value] = Rejected,
        [Failed.Value] = Failed
    };

    private ApplicationStatus(string value) : base(value)
    {
    }

    public static ApplicationStatus From(string value)
    {
        return From(value, All);
    }
}