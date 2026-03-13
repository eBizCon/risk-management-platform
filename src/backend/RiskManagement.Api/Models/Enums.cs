namespace RiskManagement.Api.Models;

public static class ApplicationStatuses
{
    public const string Draft = "draft";
    public const string Submitted = "submitted";
    public const string NeedsInformation = "needs_information";
    public const string Resubmitted = "resubmitted";
    public const string Approved = "approved";
    public const string Rejected = "rejected";

    public static readonly string[] All = { Draft, Submitted, NeedsInformation, Resubmitted, Approved, Rejected };
}

public static class EmploymentStatuses
{
    public const string Employed = "employed";
    public const string SelfEmployed = "self_employed";
    public const string Unemployed = "unemployed";
    public const string Retired = "retired";

    public static readonly string[] All = { Employed, SelfEmployed, Unemployed, Retired };
}

public static class TrafficLights
{
    public const string Red = "red";
    public const string Yellow = "yellow";
    public const string Green = "green";
}
