using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RiskManagement.Api.Models;
using RiskManagement.Api.Services;

namespace RiskManagement.Api.Data;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ScoringService _scoringService;

    private const string SeedCreatedBy = "applicant@example.com";

    private static readonly string[] Statuses = { "draft", "submitted", "approved", "rejected" };

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

    private static readonly (string Name, double Income, double FixedCosts, double DesiredRate, string EmploymentStatus, bool HasPaymentDefault)[] Templates =
    {
        ("Max Mustermann", 5200, 1900, 700, "employed", false),
        ("Sofia Wagner", 4000, 2200, 700, "self_employed", false),
        ("Jonas Becker", 3300, 2100, 500, "employed", false),
        ("Elena Fischer", 2800, 2100, 450, "unemployed", true),
        ("Noah Klein", 6100, 2400, 850, "employed", false),
        ("Mila Schmitt", 3900, 1900, 850, "retired", false),
        ("Paul Neumann", 3100, 2300, 420, "self_employed", true),
        ("Lea Hartmann", 2700, 2200, 300, "unemployed", false)
    };

    public DatabaseSeeder(ApplicationDbContext context, ScoringService scoringService)
    {
        _context = context;
        _scoringService = scoringService;
    }

    public async Task SeedAsync()
    {
        var existingCount = await _context.Applications.CountAsync();
        if (existingCount > 0)
        {
            return;
        }

        const int totalRows = 32;
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        for (var index = 0; index < totalRows; index++)
        {
            var template = Templates[index % Templates.Length];
            var status = Statuses[index % Statuses.Length];
            var scoring = _scoringService.CalculateScore(
                template.Income, template.FixedCosts, template.DesiredRate,
                template.EmploymentStatus, template.HasPaymentDefault);

            var createdAtMs = now - (totalRows - index) * 24L * 60 * 60 * 1000;
            var submittedAtMs = createdAtMs + 2L * 60 * 60 * 1000;
            var processedAtMs = submittedAtMs + 6L * 60 * 60 * 1000;

            var createdAt = DateTimeOffset.FromUnixTimeMilliseconds(createdAtMs).UtcDateTime.ToString("o");
            var submittedAt = DateTimeOffset.FromUnixTimeMilliseconds(submittedAtMs).UtcDateTime.ToString("o");
            var processedAt = DateTimeOffset.FromUnixTimeMilliseconds(processedAtMs).UtcDateTime.ToString("o");

            string? processorComment = null;
            if (status == "approved")
            {
                processorComment = ApprovedComments[index % ApprovedComments.Length];
            }
            else if (status == "rejected")
            {
                processorComment = RejectedComments[index % RejectedComments.Length];
            }

            var application = new Application
            {
                Name = $"{template.Name} {index / Templates.Length + 1}",
                Income = template.Income,
                FixedCosts = template.FixedCosts,
                DesiredRate = template.DesiredRate,
                EmploymentStatus = template.EmploymentStatus,
                HasPaymentDefault = template.HasPaymentDefault,
                Status = status,
                Score = scoring.Score,
                TrafficLight = scoring.TrafficLight,
                ScoringReasons = JsonSerializer.Serialize(scoring.Reasons),
                ProcessorComment = processorComment,
                CreatedAt = createdAt,
                SubmittedAt = status == "draft" ? null : submittedAt,
                ProcessedAt = status == "approved" || status == "rejected" ? processedAt : null,
                CreatedBy = SeedCreatedBy
            };

            _context.Applications.Add(application);
        }

        await _context.SaveChangesAsync();
    }
}
