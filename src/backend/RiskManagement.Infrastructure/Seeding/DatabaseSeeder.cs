using Microsoft.EntityFrameworkCore;
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

    private static readonly (string Name, double Income, double FixedCosts, double DesiredRate, EmploymentStatus EmploymentStatus,
        bool HasPaymentDefault)[] Templates =
        {
            ("Max Mustermann", 5200, 1900, 700, EmploymentStatus.Employed, false),
            ("Sofia Wagner", 4000, 2200, 700, EmploymentStatus.SelfEmployed, false),
            ("Jonas Becker", 3300, 2100, 500, EmploymentStatus.Employed, false),
            ("Elena Fischer", 2800, 2100, 450, EmploymentStatus.Unemployed, true),
            ("Noah Klein", 6100, 2400, 850, EmploymentStatus.Employed, false),
            ("Mila Schmitt", 3900, 1900, 850, EmploymentStatus.Retired, false),
            ("Paul Neumann", 3100, 2300, 420, EmploymentStatus.SelfEmployed, true),
            ("Lea Hartmann", 2700, 2200, 300, EmploymentStatus.Unemployed, false)
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

    public async Task SeedAsync()
    {
        await SeedScoringConfigAsync();
        await SeedApplicationsAsync();
    }

    private async Task SeedScoringConfigAsync()
    {
        var hasConfig = await _context.ScoringConfigVersions.AnyAsync();
        if (hasConfig) return;

        var defaultConfig = ScoringConfigVersion.Create(
            1,
            ScoringConfig.Default,
            EmailAddress.Create("system@risk-management.local"));

        _context.ScoringConfigVersions.Add(defaultConfig);
        await _context.SaveChangesAsync();
    }

    private async Task SeedApplicationsAsync()
    {
        var existingCount = await _context.Applications.CountAsync();
        if (existingCount > 0) return;

        var configVersion = await _context.ScoringConfigVersions
            .OrderByDescending(c => c.Version)
            .FirstAsync();

        const int totalRows = 32;
        var createdBy = EmailAddress.Create(SeedCreatedBy);

        for (var index = 0; index < totalRows; index++)
        {
            var template = Templates[index % Templates.Length];
            var targetStatus = StatusCycle[index % StatusCycle.Length];

            var customerId = index + 1;
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

        await _context.SaveChangesAsync();
    }
}