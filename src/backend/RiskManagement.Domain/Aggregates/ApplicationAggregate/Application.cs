using System.Text.Json;
using RiskManagement.Domain.Common;
using RiskManagement.Domain.Events;
using RiskManagement.Domain.Exceptions;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Domain.Aggregates.ApplicationAggregate;

public class Application : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public double Income { get; private set; }
    public double FixedCosts { get; private set; }
    public double DesiredRate { get; private set; }
    public EmploymentStatus EmploymentStatus { get; private set; } = EmploymentStatus.Employed;
    public bool HasPaymentDefault { get; private set; }
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Draft;
    public int? Score { get; private set; }
    public TrafficLight? TrafficLight { get; private set; }
    public string? ScoringReasons { get; private set; }
    public string? ProcessorComment { get; private set; }
    public string CreatedAt { get; private set; } = string.Empty;
    public string? SubmittedAt { get; private set; }
    public string? ProcessedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;

    private readonly List<ApplicationInquiry> _inquiries = new();
    public IReadOnlyList<ApplicationInquiry> Inquiries => _inquiries.AsReadOnly();

    private Application() { }

    public static Application Create(
        string name,
        double income,
        double fixedCosts,
        double desiredRate,
        EmploymentStatus employmentStatus,
        bool hasPaymentDefault,
        string createdBy,
        ScoringService scoringService)
    {
        var app = new Application
        {
            Name = name,
            Income = income,
            FixedCosts = fixedCosts,
            DesiredRate = desiredRate,
            EmploymentStatus = employmentStatus,
            HasPaymentDefault = hasPaymentDefault,
            Status = ApplicationStatus.Draft,
            CreatedAt = DateTime.UtcNow.ToString("o"),
            CreatedBy = createdBy
        };

        app.ApplyScoring(scoringService);
        return app;
    }

    public void Submit(ScoringService scoringService)
    {
        if (Status != ApplicationStatus.Draft)
            throw new InvalidStatusTransitionException(Status, ApplicationStatus.Submitted);

        ApplyScoring(scoringService);
        Status = ApplicationStatus.Submitted;
        SubmittedAt = DateTime.UtcNow.ToString("o");

        AddDomainEvent(new ApplicationSubmittedEvent(Id));
    }

    public void Approve(string? comment = null)
    {
        if (Status != ApplicationStatus.Submitted && Status != ApplicationStatus.Resubmitted)
            throw new InvalidStatusTransitionException(Status, ApplicationStatus.Approved);

        Status = ApplicationStatus.Approved;
        ProcessorComment = comment;
        ProcessedAt = DateTime.UtcNow.ToString("o");

        AddDomainEvent(new ApplicationDecidedEvent(Id, "approved"));
    }

    public void Reject(string? comment = null)
    {
        if (Status != ApplicationStatus.Submitted && Status != ApplicationStatus.Resubmitted)
            throw new InvalidStatusTransitionException(Status, ApplicationStatus.Rejected);

        Status = ApplicationStatus.Rejected;
        ProcessorComment = comment;
        ProcessedAt = DateTime.UtcNow.ToString("o");

        AddDomainEvent(new ApplicationDecidedEvent(Id, "rejected"));
    }

    public void UpdateDetails(
        string name,
        double income,
        double fixedCosts,
        double desiredRate,
        EmploymentStatus employmentStatus,
        bool hasPaymentDefault,
        ScoringService scoringService)
    {
        if (Status != ApplicationStatus.Draft)
            throw new DomainException("Nur Entwürfe können bearbeitet werden");

        Name = name;
        Income = income;
        FixedCosts = fixedCosts;
        DesiredRate = desiredRate;
        EmploymentStatus = employmentStatus;
        HasPaymentDefault = hasPaymentDefault;

        ApplyScoring(scoringService);
    }

    public void Delete()
    {
        if (Status != ApplicationStatus.Draft)
            throw new DomainException("Nur Entwürfe können gelöscht werden");

        AddDomainEvent(new ApplicationDeletedEvent(Id));
    }

    public void RequestInformation(string inquiryText, string processorEmail)
    {
        if (Status != ApplicationStatus.Submitted && Status != ApplicationStatus.Resubmitted)
            throw new InvalidStatusTransitionException(Status, ApplicationStatus.NeedsInformation);

        if (_inquiries.Any(i => i.IsOpen))
            throw new DomainException("Es gibt bereits eine offene Rückfrage");

        var inquiry = new ApplicationInquiry(Id, inquiryText, processorEmail);
        _inquiries.Add(inquiry);
        Status = ApplicationStatus.NeedsInformation;

        AddDomainEvent(new InquiryCreatedEvent(Id, inquiry.Id));
    }

    public void AnswerInquiry(string responseText)
    {
        if (Status != ApplicationStatus.NeedsInformation)
            throw new DomainException("Keine offene Rückfrage zum Beantworten");

        var openInquiry = _inquiries.FirstOrDefault(i => i.IsOpen);
        if (openInquiry is null)
            throw new DomainException("Keine offene Rückfrage gefunden");

        openInquiry.Answer(responseText);
        Status = ApplicationStatus.Resubmitted;
    }

    private void ApplyScoring(ScoringService scoringService)
    {
        var result = scoringService.CalculateScore(Income, FixedCosts, DesiredRate, EmploymentStatus, HasPaymentDefault);
        Score = result.Score;
        TrafficLight = result.TrafficLight;
        ScoringReasons = JsonSerializer.Serialize(result.Reasons);
    }
}
