namespace CustomerManagement.Api.Models;

public static class AppRoles
{
    public const string Applicant = "applicant";
    public const string Processor = "processor";
    public const string RiskManager = "risk_manager";

    public static readonly string[] All = { Applicant, Processor, RiskManager };
}

public static class AuthPolicies
{
    public const string Applicant = nameof(Applicant);
    public const string Processor = nameof(Processor);
    public const string ApplicantOrProcessor = nameof(ApplicantOrProcessor);
}