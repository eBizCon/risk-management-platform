using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Application.DTOs;

public static class ApplicationMapper
{
    public static ApplicationResponse ToResponse(ApplicationEntity application)
    {
        return new ApplicationResponse
        {
            Id = application.Id,
            Name = application.Name,
            Income = application.Income,
            FixedCosts = application.FixedCosts,
            DesiredRate = application.DesiredRate,
            EmploymentStatus = application.EmploymentStatus.Value,
            HasPaymentDefault = application.HasPaymentDefault,
            Status = application.Status.Value,
            Score = application.Score,
            TrafficLight = application.TrafficLight?.Value,
            ScoringReasons = application.ScoringReasons,
            ProcessorComment = application.ProcessorComment,
            CreatedAt = application.CreatedAt,
            SubmittedAt = application.SubmittedAt,
            ProcessedAt = application.ProcessedAt,
            CreatedBy = application.CreatedBy
        };
    }

    public static ApplicationResponse[] ToResponseArray(IEnumerable<ApplicationEntity> applications)
    {
        return applications.Select(ToResponse).ToArray();
    }
}