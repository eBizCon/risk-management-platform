namespace CustomerManagement.Application.DTOs;

public class CustomerInternalResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmploymentStatus { get; set; } = string.Empty;
    public CreditReportResponse? CreditReport { get; set; }
    public string Status { get; set; } = string.Empty;
}