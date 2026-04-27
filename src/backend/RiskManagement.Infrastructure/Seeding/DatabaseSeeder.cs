using Microsoft.EntityFrameworkCore;
using RiskManagement.Domain.ReadModels;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Domain.ValueObjects;
using RiskManagement.Infrastructure.Persistence;
using SharedKernel.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Infrastructure.Seeding;

public class DatabaseSeeder
{
    public sealed record SeededCustomer(int Id, string FirstName, string LastName, string Status);

    private readonly ApplicationDbContext _context;
    private readonly IScoringService _scoringService;

    private const string SeedCreatedBy = "applicant@example.com";

    private static readonly string[] ApprovedComments =
    {
        "Bonität geprüft, Antrag genehmigt.",
        "Einkommensverhältnisse wurden positiv bewertet.",
        "Antrag nach manueller Prüfung freigegeben.",
        "Risiko ist gering, Genehmigung erteilt."
    };

    private static readonly string[] RejectedComments =
    {
        "Zu hohes Risiko aufgrund bestehender Zahlungsverzüge.",
        "Verfügbares Einkommen reicht für die gewünschte Rate nicht aus.",
        "Kritisches Verhältnis zwischen Einkommen und Fixkosten.",
        "Antrag aufgrund negativer Gesamtbewertung abgelehnt."
    };

    private static readonly (double Income, double FixedCosts, double DesiredRate, EmploymentStatus
        EmploymentStatus,
        bool HasPaymentDefault)[] Templates =
        {
            (5200, 1900, 700, EmploymentStatus.Employed, false),
            (4000, 2200, 700, EmploymentStatus.SelfEmployed, false),
            (3300, 2100, 500, EmploymentStatus.Employed, false),
            (2800, 2100, 450, EmploymentStatus.Unemployed, true),
            (6100, 2400, 850, EmploymentStatus.Employed, false),
            (3900, 1900, 850, EmploymentStatus.Retired, false),
            (3100, 2300, 420, EmploymentStatus.SelfEmployed, true),
            (2700, 2200, 300, EmploymentStatus.Unemployed, false)
        };

    private enum SeedStatus
    {
        Draft,
        Submitted,
        Approved,
        Rejected
    }

    private static readonly SeedStatus[] StatusCycle =
        { SeedStatus.Draft, SeedStatus.Submitted, SeedStatus.Approved, SeedStatus.Rejected };

    public DatabaseSeeder(ApplicationDbContext context, IScoringService scoringService)
    {
        _context = context;
        _scoringService = scoringService;
    }

    public async Task SeedAsync(IReadOnlyList<SeededCustomer> customers, CancellationToken ct = default)
    {
        if (customers.Count == 0)
        {
            throw new InvalidOperationException("At least one customer is required for risk seeding.");
        }

        await SeedCustomerReadModelsAsync(customers, ct);
        await SeedScoringConfigAsync(ct);
        await SeedApplicationsAsync(customers.Select(customer => customer.Id).ToList(), ct);
    }

    private async Task SeedCustomerReadModelsAsync(IReadOnlyList<SeededCustomer> customers, CancellationToken ct)
    {
        var hasReadModels = await _context.CustomerReadModels.AnyAsync(ct);
        if (hasReadModels)
        {
            return;
        }

        var now = DateTime.UtcNow;

        foreach (var customer in customers)
        {
            _context.CustomerReadModels.Add(new CustomerReadModel
            {
                CustomerId = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Status = customer.Status,
                LastUpdatedAt = now
            });
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedScoringConfigAsync(CancellationToken ct)
    {
        var hasConfig = await _context.ScoringConfigVersions.AnyAsync(ct);
        if (hasConfig) return;

        var defaultConfig = ScoringConfigVersion.Create(
            1,
            ScoringConfig.Default,
            EmailAddress.Create("system@risk-management.local"));

        _context.ScoringConfigVersions.Add(defaultConfig);
        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedApplicationsAsync(IReadOnlyList<int> customerIds, CancellationToken ct)
    {
        var existingCount = await _context.Applications.CountAsync(ct);
        if (existingCount > 0) return;

        var configVersion = await _context.ScoringConfigVersions
            .OrderByDescending(c => c.Version)
            .FirstAsync(ct);

        const int totalRows = 32;
        var createdBy = EmailAddress.Create(SeedCreatedBy);

        for (var index = 0; index < totalRows; index++)
        {
            var template = Templates[index % Templates.Length];
            var targetStatus = StatusCycle[index % StatusCycle.Length];

            var customerId = customerIds[index % customerIds.Count];
            var creditReport = CreditReport.Create(
                template.HasPaymentDefault,
                420,
                DateTime.UtcNow,
                "schufa_mock");

            var application = ApplicationEntity.Create(
                customerId,
                Money.Create((decimal)template.Income),
                Money.Create((decimal)template.FixedCosts),
                Money.CreatePositive((decimal)template.DesiredRate),
                template.EmploymentStatus,
                creditReport,
                createdBy,
                _scoringService,
                configVersion.Config,
                configVersion.Id);

            if (targetStatus != SeedStatus.Draft)
            {
                application.Submit(_scoringService, configVersion.Config, configVersion.Id);

                if (targetStatus == SeedStatus.Approved)
                    application.Approve(ApprovedComments[index % ApprovedComments.Length]);
                else if (targetStatus == SeedStatus.Rejected)
                    application.Reject(RejectedComments[index % RejectedComments.Length]);
            }

            application.ClearDomainEvents();
            _context.Applications.Add(application);
        }

        await _context.SaveChangesAsync(ct);
    }
}