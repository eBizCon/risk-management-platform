using System.Text.Json;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Common;
using RiskManagement.Domain.Events;
using RiskManagement.Domain.Exceptions;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;

namespace RiskManagement.Domain.Aggregates.ApplicationAggregate;

public class Application : AggregateRoot<ApplicationId>
{
    public int CustomerId { get; private set; }
    public Money Income { get; private set; } = Money.Zero;
    public Money FixedCosts { get; private set; } = Money.Zero;
    public Money DesiredRate { get; private set; } = Money.Zero;
    public EmploymentStatus EmploymentStatus { get; private set; } = EmploymentStatus.Employed;
    public CreditReport CreditReport { get; private set; } = null!;
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Draft;
    public int? Score { get; private set; }
    public TrafficLight? TrafficLight { get; private set; }
    public string? ScoringReasons { get; private set; }
    public ScoringConfigVersionId? ScoringConfigVersionId { get; private set; }
    public string? ProcessorComment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public EmailAddress CreatedBy { get; private set; } = null!;

    private readonly List<ApplicationInquiry> _inquiries = new();
    public IReadOnlyList<ApplicationInquiry> Inquiries => _inquiries.AsReadOnly();

    private Application()
    {
    }

    public static Application Create(
        int customerId,
        Money income,
        Money fixedCosts,
        Money desiredRate,
        EmploymentStatus employmentStatus,
        CreditReport creditReport,
        EmailAddress createdBy,
        IScoringService scoringService,
        ScoringConfig scoringConfig,
        ScoringConfigVersionId scoringConfigVersionId)
    {
        if (customerId <= 0)
            throw new DomainException("Kunde muss ausgewählt werden");

        if (income <= Money.Zero)
            throw new DomainException("Einkommen muss positiv sein");

        if (desiredRate <= Money.Zero)
            throw new DomainException("Gewünschte Rate muss positiv sein");

        if (fixedCosts >= income)
            throw new DomainException("Fixkosten müssen geringer als das Einkommen sein");

        if (desiredRate > income - fixedCosts)
            throw new DomainException("Gewünschte Rate darf das verfügbare Einkommen nicht übersteigen");

        var app = new Application
        {
            CustomerId = customerId,
            Income = income,
            FixedCosts = fixedCosts,
            DesiredRate = desiredRate,
            EmploymentStatus = employmentStatus,
            CreditReport = creditReport,
            Status = ApplicationStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        app.ApplyScoring(scoringService, scoringConfig, scoringConfigVersionId);
        return app;
    }

    public void Submit(IScoringService scoringService, ScoringConfig scoringConfig,
        ScoringConfigVersionId scoringConfigVersionId)
    {
        if (Status != ApplicationStatus.Draft)
            throw new InvalidStatusTransitionException(Status, ApplicationStatus.Submitted);

        ApplyScoring(scoringService, scoringConfig, scoringConfigVersionId);
        Status = ApplicationStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;

        AddDomainEvent(new ApplicationSubmittedEvent(Id));
    }

    public void Approve(string? comment = null)
    {
        if (Status != ApplicationStatus.Submitted && Status != ApplicationStatus.Resubmitted)
            throw new InvalidStatusTransitionException(Status, ApplicationStatus.Approved);

        Status = ApplicationStatus.Approved;
        ProcessorComment = comment;
        ProcessedAt = DateTime.UtcNow;

        AddDomainEvent(new ApplicationDecidedEvent(Id, "approved"));
    }

    public void Reject(string? comment = null)
    {
        if (Status != ApplicationStatus.Submitted && Status != ApplicationStatus.Resubmitted)
            throw new InvalidStatusTransitionException(Status, ApplicationStatus.Rejected);

        Status = ApplicationStatus.Rejected;
        ProcessorComment = comment;
        ProcessedAt = DateTime.UtcNow;

        AddDomainEvent(new ApplicationDecidedEvent(Id, "rejected"));
    }

    public void UpdateDetails(
        int customerId,
        Money income,
        Money fixedCosts,
        Money desiredRate,
        EmploymentStatus employmentStatus,
        CreditReport creditReport,
        IScoringService scoringService,
        ScoringConfig scoringConfig,
        ScoringConfigVersionId scoringConfigVersionId)
    {
        if (Status != ApplicationStatus.Draft)
            throw new DomainException("Nur Entwürfe können bearbeitet werden");

        if (customerId <= 0)
            throw new DomainException("Kunde muss ausgewählt werden");

        if (income <= Money.Zero)
            throw new DomainException("Einkommen muss positiv sein");

        if (desiredRate <= Money.Zero)
            throw new DomainException("Gewünschte Rate muss positiv sein");

        if (fixedCosts >= income)
            throw new DomainException("Fixkosten müssen geringer als das Einkommen sein");

        if (desiredRate > income - fixedCosts)
            throw new DomainException("Gewünschte Rate darf das verfügbare Einkommen nicht übersteigen");

        CustomerId = customerId;
        Income = income;
        FixedCosts = fixedCosts;
        DesiredRate = desiredRate;
        EmploymentStatus = employmentStatus;
        CreditReport = creditReport;

        ApplyScoring(scoringService, scoringConfig, scoringConfigVersionId);
    }

    public void Rescore(IScoringService scoringService, ScoringConfig scoringConfig,
        ScoringConfigVersionId scoringConfigVersionId)
    {
        ApplyScoring(scoringService, scoringConfig, scoringConfigVersionId);
    }

    public void Delete()
    {
        if (Status != ApplicationStatus.Draft)
            throw new DomainException("Nur Entwürfe können gelöscht werden");

        AddDomainEvent(new ApplicationDeletedEvent(Id));
    }

    public void RequestInformation(string inquiryText, EmailAddress processorEmail)
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

    private void ApplyScoring(IScoringService scoringService, ScoringConfig scoringConfig,
        ScoringConfigVersionId scoringConfigVersionId)
    {
        var result =
            scoringService.CalculateScore(Income, FixedCosts, DesiredRate, EmploymentStatus,
                CreditReport.HasPaymentDefault, scoringConfig);
        Score = result.Score;
        TrafficLight = result.TrafficLight;
        ScoringReasons = JsonSerializer.Serialize(result.Reasons);
        ScoringConfigVersionId = scoringConfigVersionId;
    }
}