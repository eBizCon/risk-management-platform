namespace RiskManagement.Application.DTOs;

public class ApplicationCreateDto
{
    public int CustomerId { get; set; }
    public double Income { get; set; }
    public double FixedCosts { get; set; }
    public double DesiredRate { get; set; }
}

public class ApplicationUpdateDto
{
    public int CustomerId { get; set; }
    public double Income { get; set; }
    public double FixedCosts { get; set; }
    public double DesiredRate { get; set; }
}

public class ApproveApplicationDto
{
    public string? Comment { get; set; }
}

public class RejectApplicationDto
{
    public string Comment { get; set; } = string.Empty;
}

public class InquiryCreateDto
{
    public string InquiryText { get; set; } = string.Empty;
}

public class InquiryResponseDto
{
    public string ResponseText { get; set; } = string.Empty;
}

public class ApplicationResponse
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public double Income { get; set; }
    public double FixedCosts { get; set; }
    public double DesiredRate { get; set; }
    public string EmploymentStatus { get; set; } = string.Empty;
    public bool? HasPaymentDefault { get; set; }
    public int? CreditScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string? TrafficLight { get; set; }
    public string? ScoringReasons { get; set; }
    public string? ProcessorComment { get; set; }
    public string? FailureReason { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? SubmittedAt { get; set; }
    public string? ProcessedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class ProcessorApplicationsResponse
{
    public ApplicationResponse[] Applications { get; set; } = Array.Empty<ApplicationResponse>();
    public string? StatusFilter { get; set; }
    public ProcessorStatsDto Stats { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
}

public class ProcessorStatsDto
{
    public int Total { get; set; }
    public int Submitted { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
}

public class PaginationInfo
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

public class ValidationErrorResponse
{
    public Dictionary<string, string[]> Errors { get; set; } = new();
    public object? Values { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class DashboardStatsDto
{
    public int Draft { get; set; }
    public int Submitted { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
}

public class TestSessionCreateDto
{
    public string Role { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}