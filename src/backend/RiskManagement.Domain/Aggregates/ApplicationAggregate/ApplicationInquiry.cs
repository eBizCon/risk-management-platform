using RiskManagement.Domain.Common;

namespace RiskManagement.Domain.Aggregates.ApplicationAggregate;

public class ApplicationInquiry : Entity
{
    public int ApplicationId { get; private set; }
    public string InquiryText { get; private set; } = string.Empty;
    public string Status { get; private set; } = "open";
    public string ProcessorEmail { get; private set; } = string.Empty;
    public string? ResponseText { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }

    private ApplicationInquiry()
    {
    }

    internal ApplicationInquiry(int applicationId, string inquiryText, string processorEmail)
    {
        ApplicationId = applicationId;
        InquiryText = inquiryText;
        ProcessorEmail = processorEmail;
        Status = "open";
        CreatedAt = DateTime.UtcNow;
    }

    internal void Answer(string responseText)
    {
        ResponseText = responseText;
        RespondedAt = DateTime.UtcNow;
        Status = "answered";
    }

    public bool IsOpen => Status == "open";
}