using SharedKernel.Common;

namespace SharedKernel.ValueObjects;

public sealed class EmploymentStatus : Enumeration<EmploymentStatus>
{
    public static readonly EmploymentStatus Employed = new("employed");
    public static readonly EmploymentStatus SelfEmployed = new("self_employed");
    public static readonly EmploymentStatus Unemployed = new("unemployed");
    public static readonly EmploymentStatus Retired = new("retired");

    private static readonly Dictionary<string, EmploymentStatus> All = new()
    {
        [Employed.Value] = Employed,
        [SelfEmployed.Value] = SelfEmployed,
        [Unemployed.Value] = Unemployed,
        [Retired.Value] = Retired
    };

    private EmploymentStatus(string value) : base(value)
    {
    }

    public static EmploymentStatus From(string value)
    {
        return From(value, All);
    }

    public static string[] AllValues => All.Keys.ToArray();
}
