namespace CustomerManagement.Application.DTOs;

public class CreditReportResponse
{
    public bool HasPaymentDefault { get; set; }
    public int? CreditScore { get; set; }
    public string CheckedAt { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
}
