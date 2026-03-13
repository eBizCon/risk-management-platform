using RiskManagement.Domain.Common;

namespace RiskManagement.Domain.ValueObjects;

public sealed class InquiryStatus : Enumeration<InquiryStatus>
{
    public static readonly InquiryStatus Open = new("open");
    public static readonly InquiryStatus Answered = new("answered");

    private static readonly Dictionary<string, InquiryStatus> All = new()
    {
        [Open.Value] = Open,
        [Answered.Value] = Answered
    };

    private InquiryStatus(string value) : base(value)
    {
    }

    public static InquiryStatus From(string value) => From(value, All);
}
