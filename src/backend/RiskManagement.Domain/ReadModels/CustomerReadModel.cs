namespace RiskManagement.Domain.ReadModels;

public class CustomerReadModel
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime LastUpdatedAt { get; set; }
}