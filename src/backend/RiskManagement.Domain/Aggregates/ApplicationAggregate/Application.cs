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
    public Money? LoanAmount { get; private set; }
    public int? LoanTerm { get; private set; }
    public EmploymentStatus EmploymentStatus { get; private set; } = EmploymentStatus.Employed;
    public CreditReport CreditReport { get; private set; } = null!;
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Draft;
    public int? Score { get; private set; }
    public TrafficLight? TrafficLight { get; private set; }
    public string? ScoringReasons { get; private set; }
    public ScoringConfigVersionId? ScoringConfigVersionId { get; private set; }
    public string? ProcessorComment { get; private set; }
    public string? FailureReason { get; private set; }
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
        Money loanAmount,
        int loanTerm,
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

        if (loanAmount <= Money.Zero)
            throw new DomainException("Kreditbetrag muss positiv sein");

        if (loanTerm <= 0 || loanTerm > 360)
            throw new DomainException("Laufzeit muss zwischen 1 und 360 Monaten liegen");

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
            LoanAmount = loanAmount,
            LoanTerm = loanTerm,
            EmploymentStatus = employmentStatus,
            CreditReport = creditReport,
            Status = ApplicationStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        app.ApplyScoring(scoringService, scoringConfig, scoringConfigVersionId);
        return app;
    }

    public static Application CreateProcessing(
        int customerId,
        Money income,
        Money fixedCosts,
        Money desiredRate,
        Money loanAmount,
        int loanTerm,
        EmailAddress createdBy)
    {
        if (customerId <= 0)
            throw new DomainException("Kunde muss ausgewählt werden");

        if (income <= Money.Zero)
            throw new DomainException("Einkommen muss positiv sein");

        if (desiredRate <= Money.Zero)
            throw new DomainException("Gewünschte Rate muss positiv sein");

        if (loanAmount <= Money.Zero)
            throw new DomainException("Kreditbetrag muss positiv sein");

        if (loanTerm <= 0 || loanTerm > 360)
            throw new DomainException("Laufzeit muss zwischen 1 und 360 Monaten liegen");

        if (fixedCosts >= income)
            throw new DomainException("Fixkosten müssen geringer als das Einkommen sein");

        if (desiredRate > income - fixedCosts)
            throw new DomainException("Gewünschte Rate darf das verfügbare Einkommen nicht übersteigen");

        return new Application
        {
            CustomerId = customerId,
            Income = income,
            FixedCosts = fixedCosts,
            DesiredRate = desiredRate,
            LoanAmount = loanAmount,
            LoanTerm = loanTerm,
            Status = ApplicationStatus.Processing,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void Finalize(
        EmploymentStatus employmentStatus,
        CreditReport creditReport,
        IScoringService scoringService,
        ScoringConfig scoringConfig,
        ScoringConfigVersionId scoringConfigVersionId)
    {
        if (Status != ApplicationStatus.Processing)
            throw new DomainException("Nur Anträge im Status 'Verarbeitung' können finalisiert werden");

        EmploymentStatus = employmentStatus;
        CreditReport = creditReport;
        ApplyScoring(scoringService, scoringConfig, scoringConfigVersionId);
        Status = ApplicationStatus.Draft;
    }

    public void SetProcessing()
    {
        if (Status != ApplicationStatus.Draft)
            throw new DomainException("Nur Entwürfe können in den Verarbeitungsstatus versetzt werden");

        Status = ApplicationStatus.Processing;
    }

    public void MarkFailed(string reason)
    {
        if (Status != ApplicationStatus.Processing)
            throw new DomainException("Nur Anträge im Status 'Verarbeitung' können als fehlgeschlagen markiert werden");

        FailureReason = reason;
        Status = ApplicationStatus.Failed;
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

        if (TrafficLight != ValueObjects.TrafficLight.Green && string.IsNullOrWhiteSpace(comment))
            throw new DomainException("Bei Ampelstatus Gelb oder Rot ist eine Begründung für die Genehmigung erforderlich");

        Status = ApplicationStatus.Approved;
        ProcessorComment = comment;
        ProcessedAt = DateTime.UtcNow;

        AddDomainEvent(new ApplicationDecidedEvent(Id, DecisionType.Approved));
    }

    public void Reject(string? comment = null)
    {
        if (Status != ApplicationStatus.Submitted && Status != ApplicationStatus.Resubmitted)
            throw new InvalidStatusTransitionException(Status, ApplicationStatus.Rejected);

        if (TrafficLight == ValueObjects.TrafficLight.Green && string.IsNullOrWhiteSpace(comment))
            throw new DomainException("Bei Ampelstatus Grün ist eine Begründung für die Ablehnung erforderlich");

        Status = ApplicationStatus.Rejected;
        ProcessorComment = comment;
        ProcessedAt = DateTime.UtcNow;

        AddDomainEvent(new ApplicationDecidedEvent(Id, DecisionType.Rejected));
    }

    public void UpdateDetails(
        int customerId,
        Money income,
        Money fixedCosts,
        Money desiredRate,
        Money loanAmount,
        int loanTerm,
        EmploymentStatus employmentStatus,
        CreditReport creditReport,
        IScoringService scoringService,
        ScoringConfig scoringConfig,
        ScoringConfigVersionId scoringConfigVersionId)
    {
        if (Status != ApplicationStatus.Draft && Status != ApplicationStatus.Processing)
            throw new DomainException("Nur Entwürfe und Anträge in Verarbeitung können bearbeitet werden");

        if (customerId <= 0)
            throw new DomainException("Kunde muss ausgewählt werden");

        if (income <= Money.Zero)
            throw new DomainException("Einkommen muss positiv sein");

        if (desiredRate <= Money.Zero)
            throw new DomainException("Gewünschte Rate muss positiv sein");

        if (loanAmount <= Money.Zero)
            throw new DomainException("Kreditbetrag muss positiv sein");

        if (loanTerm <= 0 || loanTerm > 360)
            throw new DomainException("Laufzeit muss zwischen 1 und 360 Monaten liegen");

        if (fixedCosts >= income)
            throw new DomainException("Fixkosten müssen geringer als das Einkommen sein");

        if (desiredRate > income - fixedCosts)
            throw new DomainException("Gewünschte Rate darf das verfügbare Einkommen nicht übersteigen");

        CustomerId = customerId;
        Income = income;
        FixedCosts = fixedCosts;
        DesiredRate = desiredRate;
        LoanAmount = loanAmount;
        LoanTerm = loanTerm;
        EmploymentStatus = employmentStatus;
        CreditReport = creditReport;

        ApplyScoring(scoringService, scoringConfig, scoringConfigVersionId);

        if (Status == ApplicationStatus.Processing)
            Status = ApplicationStatus.Draft;
    }

    public void Rescore(IScoringService scoringService, ScoringConfig scoringConfig,
        ScoringConfigVersionId scoringConfigVersionId)
    {
        if (Status == ApplicationStatus.Approved ||
            Status == ApplicationStatus.Rejected ||
            Status == ApplicationStatus.Failed ||
            Status == ApplicationStatus.Processing)
            throw new DomainException("Abgeschlossene oder in Verarbeitung befindliche Anträge können nicht neu bewertet werden");

        ApplyScoring(scoringService, scoringConfig, scoringConfigVersionId);
    }

    public void Delete()
    {
        if (Status != ApplicationStatus.Draft && Status != ApplicationStatus.Failed)
            throw new DomainException("Nur Entwürfe und fehlgeschlagene Anträge können gelöscht werden");

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
        var hasPaymentDefault = CreditReport?.HasPaymentDefault
                                ?? throw new DomainException("CreditReport muss vorhanden sein für Scoring");

        var creditScore = CreditReport?.CreditScore;
        var result =
            scoringService.CalculateScore(Income, FixedCosts, DesiredRate, EmploymentStatus,
                hasPaymentDefault, creditScore, scoringConfig, LoanAmount, LoanTerm);
        Score = result.Score;
        TrafficLight = result.TrafficLight;
        ScoringReasons = JsonSerializer.Serialize(result.Reasons);
        ScoringConfigVersionId = scoringConfigVersionId;
    }
}
