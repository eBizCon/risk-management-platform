namespace RiskManagement.Api.Models;

public class Application
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Income { get; set; }
    public double FixedCosts { get; set; }
    public double DesiredRate { get; set; }
    public string EmploymentStatus { get; set; } = string.Empty;
    public bool HasPaymentDefault { get; set; }
    public string Status { get; set; } = "draft";
    public int? Score { get; set; }
    public string? TrafficLight { get; set; }
    public string? ScoringReasons { get; set; }
    public string? ProcessorComment { get; set; }
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    public string? SubmittedAt { get; set; }
    public string? ProcessedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
