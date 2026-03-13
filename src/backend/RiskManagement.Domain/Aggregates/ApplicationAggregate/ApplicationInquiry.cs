using RiskManagement.Domain.Common;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Domain.Aggregates.ApplicationAggregate;

public class ApplicationInquiry : Entity<InquiryId>
{
    public ApplicationId ApplicationId { get; private set; }
    public string InquiryText { get; private set; } = string.Empty;
    public string? ResponseText { get; private set; }
    public InquiryStatus Status { get; private set; } = InquiryStatus.Open;
    public EmailAddress ProcessorEmail { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? AnsweredAt { get; private set; }

    public bool IsOpen => Status == InquiryStatus.Open;

    private ApplicationInquiry()
    {
    }

    public ApplicationInquiry(ApplicationId applicationId, string inquiryText, EmailAddress processorEmail)
    {
        ApplicationId = applicationId;
        InquiryText = inquiryText;
        ProcessorEmail = processorEmail;
        Status = InquiryStatus.Open;
        CreatedAt = DateTime.UtcNow;
    }

    public void Answer(string responseText)
    {
        ResponseText = responseText;
        Status = InquiryStatus.Answered;
        AnsweredAt = DateTime.UtcNow;
    }
}