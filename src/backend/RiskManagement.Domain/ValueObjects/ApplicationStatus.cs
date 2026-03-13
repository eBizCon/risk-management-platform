namespace RiskManagement.Domain.ValueObjects;

public sealed class ApplicationStatus : IEquatable<ApplicationStatus>
{
    public static readonly ApplicationStatus Draft = new("draft");
    public static readonly ApplicationStatus Submitted = new("submitted");
    public static readonly ApplicationStatus NeedsInformation = new("needs_information");
    public static readonly ApplicationStatus Resubmitted = new("resubmitted");
    public static readonly ApplicationStatus Approved = new("approved");
    public static readonly ApplicationStatus Rejected = new("rejected");

    private static readonly Dictionary<string, ApplicationStatus> All = new()
    {
        [Draft.Value] = Draft,
        [Submitted.Value] = Submitted,
        [NeedsInformation.Value] = NeedsInformation,
        [Resubmitted.Value] = Resubmitted,
        [Approved.Value] = Approved,
        [Rejected.Value] = Rejected
    };

    public string Value { get; }

    private ApplicationStatus(string value)
    {
        Value = value;
    }

    public static ApplicationStatus From(string value)
    {
        if (All.TryGetValue(value, out var status))
            return status;

        throw new ArgumentException($"Invalid ApplicationStatus: '{value}'", nameof(value));
    }

    public override string ToString() => Value;

    public bool Equals(ApplicationStatus? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is ApplicationStatus other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ApplicationStatus? left, ApplicationStatus? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(ApplicationStatus? left, ApplicationStatus? right) => !(left == right);
}
