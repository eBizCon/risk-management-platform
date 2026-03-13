using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Application.DTOs;

public static class ApplicationMapper
{
    public static ApplicationResponse ToResponse(ApplicationEntity application)
    {
        return new ApplicationResponse
        {
            Id = application.Id.Value,
            CustomerId = application.CustomerId,
            Income = (double)application.Income.Amount,
            FixedCosts = (double)application.FixedCosts.Amount,
            DesiredRate = (double)application.DesiredRate.Amount,
            EmploymentStatus = application.EmploymentStatus.Value,
            HasPaymentDefault = application.HasPaymentDefault,
            Status = application.Status.Value,
            Score = application.Score,
            TrafficLight = application.TrafficLight?.Value,
            ScoringReasons = application.ScoringReasons,
            ProcessorComment = application.ProcessorComment,
            CreatedAt = application.CreatedAt.ToString("o"),
            SubmittedAt = application.SubmittedAt?.ToString("o"),
            ProcessedAt = application.ProcessedAt?.ToString("o"),
            CreatedBy = application.CreatedBy.Value
        };
    }

    public static ApplicationResponse[] ToResponseArray(IEnumerable<ApplicationEntity> applications)
    {
        return applications.Select(ToResponse).ToArray();
    }
}