namespace RiskManagement.Api.Services;

public class ScoringResult
{
    public int Score { get; set; }
    public string TrafficLight { get; set; } = string.Empty;
    public List<string> Reasons { get; set; } = new();
}

public class ScoringService
{
    public ScoringResult CalculateScore(
        double income,
        double fixedCosts,
        double desiredRate,
        string employmentStatus,
        bool hasPaymentDefault)
    {
        var reasons = new List<string>();
        var score = 100;

        var availableIncome = income - fixedCosts;
        var incomeRatio = availableIncome / income;

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

        var rateToAvailableRatio = desiredRate / availableIncome;

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

        switch (employmentStatus)
        {
            case "employed":
                reasons.Add("Angestelltenverhältnis bietet stabile Einkommenssituation");
                break;
            case "self_employed":
                score -= 10;
                reasons.Add("Selbstständigkeit birgt gewisses Einkommensrisiko");
                break;
            case "retired":
                score -= 5;
                reasons.Add("Ruhestand bietet stabile, aber begrenzte Einkommenssituation");
                break;
            case "unemployed":
                score -= 35;
                reasons.Add("Arbeitslosigkeit stellt erhebliches Risiko für Kreditrückzahlung dar");
                break;
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

        string trafficLight;
        if (score >= 75)
        {
            trafficLight = "green";
            reasons.Insert(0, "Gesamtbewertung: Positiv - Kreditantrag empfohlen");
        }
        else if (score >= 50)
        {
            trafficLight = "yellow";
            reasons.Insert(0, "Gesamtbewertung: Prüfung erforderlich - manuelle Bewertung empfohlen");
        }
        else
        {
            trafficLight = "red";
            reasons.Insert(0, "Gesamtbewertung: Kritisch - erhöhtes Risiko");
        }

        return new ScoringResult
        {
            Score = score,
            TrafficLight = trafficLight,
            Reasons = reasons
        };
    }
}
