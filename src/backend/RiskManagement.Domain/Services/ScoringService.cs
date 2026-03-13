using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Domain.Services;

public class ScoringService : IScoringService
{
    public ScoringResult CalculateScore(
        Money income,
        Money fixedCosts,
        Money desiredRate,
        EmploymentStatus employmentStatus,
        bool hasPaymentDefault)
    {
        var reasons = new List<string>();
        var score = 100;

        var incomeAmount = (double)income.Amount;
        var fixedCostsAmount = (double)fixedCosts.Amount;
        var desiredRateAmount = (double)desiredRate.Amount;

        var availableIncome = incomeAmount - fixedCostsAmount;
        var incomeRatio = availableIncome / incomeAmount;

        if (incomeRatio >= 0.5)
        {
            reasons.Add("Gutes Verhältnis zwischen Einkommen und Fixkosten (mehr als 50% verfügbar)");
        }
        else if (incomeRatio >= 0.3)
        {
            score -= 15;
            reasons.Add("Moderates Verhältnis zwischen Einkommen und Fixkosten (30-50% verfügbar)");
        }
        else if (incomeRatio >= 0.1)
        {
            score -= 30;
            reasons.Add("Eingeschränktes Verhältnis zwischen Einkommen und Fixkosten (10-30% verfügbar)");
        }
        else
        {
            score -= 50;
            reasons.Add("Kritisches Verhältnis zwischen Einkommen und Fixkosten (weniger als 10% verfügbar)");
        }

        var rateToAvailableRatio = desiredRateAmount / availableIncome;

        if (rateToAvailableRatio <= 0.3)
        {
            reasons.Add("Gewünschte Rate ist gut tragbar (maximal 30% des verfügbaren Einkommens)");
        }
        else if (rateToAvailableRatio <= 0.5)
        {
            score -= 10;
            reasons.Add("Gewünschte Rate ist moderat tragbar (30-50% des verfügbaren Einkommens)");
        }
        else if (rateToAvailableRatio <= 0.7)
        {
            score -= 25;
            reasons.Add("Gewünschte Rate belastet das Budget erheblich (50-70% des verfügbaren Einkommens)");
        }
        else
        {
            score -= 40;
            reasons.Add("Gewünschte Rate übersteigt das tragbare Maß (mehr als 70% des verfügbaren Einkommens)");
        }

        if (employmentStatus == EmploymentStatus.Employed)
        {
            reasons.Add("Angestelltenverhältnis bietet stabile Einkommenssituation");
        }
        else if (employmentStatus == EmploymentStatus.SelfEmployed)
        {
            score -= 10;
            reasons.Add("Selbstständigkeit birgt gewisses Einkommensrisiko");
        }
        else if (employmentStatus == EmploymentStatus.Retired)
        {
            score -= 5;
            reasons.Add("Ruhestand bietet stabile, aber begrenzte Einkommenssituation");
        }
        else if (employmentStatus == EmploymentStatus.Unemployed)
        {
            score -= 35;
            reasons.Add("Arbeitslosigkeit stellt erhebliches Risiko für Kreditrückzahlung dar");
        }

        if (hasPaymentDefault)
        {
            score -= 25;
            reasons.Add("Frühere Zahlungsverzüge beeinträchtigen die Kreditwürdigkeit");
        }
        else
        {
            reasons.Add("Keine früheren Zahlungsverzüge - positive Zahlungshistorie");
        }

        score = Math.Max(0, Math.Min(100, score));

        TrafficLight trafficLight;
        if (score >= 75)
        {
            trafficLight = TrafficLight.Green;
            reasons.Insert(0, "Gesamtbewertung: Positiv - Kreditantrag empfohlen");
        }
        else if (score >= 50)
        {
            trafficLight = TrafficLight.Yellow;
            reasons.Insert(0, "Gesamtbewertung: Prüfung erforderlich - manuelle Bewertung empfohlen");
        }
        else
        {
            trafficLight = TrafficLight.Red;
            reasons.Insert(0, "Gesamtbewertung: Kritisch - erhöhtes Risiko");
        }

        return new ScoringResult(score, trafficLight, reasons);
    }
}