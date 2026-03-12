namespace RiskManagement.Api.Models;

public class ApplicationCreateDto
{
    public string Name { get; set; } = string.Empty;
    public double Income { get; set; }
    public double FixedCosts { get; set; }
    public double DesiredRate { get; set; }
    public string EmploymentStatus { get; set; } = string.Empty;
    public bool HasPaymentDefault { get; set; }
    public string? Action { get; set; }
}

public class ApplicationUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public double Income { get; set; }
    public double FixedCosts { get; set; }
    public double DesiredRate { get; set; }
    public string EmploymentStatus { get; set; } = string.Empty;
    public bool HasPaymentDefault { get; set; }
    public string? Action { get; set; }
}

public class ProcessorDecisionDto
{
    public string Decision { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

public class TestSessionCreateDto
{
    public string Role { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class ProcessorApplicationsResponse
{
    public Application[] Applications { get; set; } = Array.Empty<Application>();
    public string? StatusFilter { get; set; }
    public ProcessorStats Stats { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
}

public class ProcessorStats
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
