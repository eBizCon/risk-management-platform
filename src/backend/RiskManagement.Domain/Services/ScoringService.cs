using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;

namespace RiskManagement.Domain.Services;

public class ScoringService : IScoringService
{
    public ScoringResult CalculateScore(
        Money income,
        Money fixedCosts,
        Money desiredRate,
        EmploymentStatus employmentStatus,
        bool hasPaymentDefault,
        int? creditScore,
        ScoringConfig config,
        Money? loanAmount = null,
        int? loanTerm = null)
    {
        var reasons = new List<string>();
        var score = 100;

        var incomeAmount = (double)income.Amount;
        var fixedCostsAmount = (double)fixedCosts.Amount;
        var desiredRateAmount = (double)desiredRate.Amount;

        var availableIncome = incomeAmount - fixedCostsAmount;
        var incomeRatio = availableIncome / incomeAmount;

        if (incomeRatio >= (double)config.IncomeRatioGood)
        {
            reasons.Add("Gutes Verhältnis zwischen Einkommen und Fixkosten (mehr als 50% verfügbar)");
        }
        else if (incomeRatio >= (double)config.IncomeRatioModerate)
        {
            score -= config.PenaltyModerateRatio;
            reasons.Add("Moderates Verhältnis zwischen Einkommen und Fixkosten (30-50% verfügbar)");
        }
        else if (incomeRatio >= (double)config.IncomeRatioLimited)
        {
            score -= config.PenaltyLimitedRatio;
            reasons.Add("Eingeschränktes Verhältnis zwischen Einkommen und Fixkosten (10-30% verfügbar)");
        }
        else
        {
            score -= config.PenaltyCriticalRatio;
            reasons.Add("Kritisches Verhältnis zwischen Einkommen und Fixkosten (weniger als 10% verfügbar)");
        }

        var rateToAvailableRatio = desiredRateAmount / availableIncome;

        if (rateToAvailableRatio <= (double)config.RateGood)
        {
            reasons.Add("Gewünschte Rate ist gut tragbar (maximal 30% des verfügbaren Einkommens)");
        }
        else if (rateToAvailableRatio <= (double)config.RateModerate)
        {
            score -= config.PenaltyModerateRate;
            reasons.Add("Gewünschte Rate ist moderat tragbar (30-50% des verfügbaren Einkommens)");
        }
        else if (rateToAvailableRatio <= (double)config.RateHeavy)
        {
            score -= config.PenaltyHeavyRate;
            reasons.Add("Gewünschte Rate belastet das Budget erheblich (50-70% des verfügbaren Einkommens)");
        }
        else
        {
            score -= config.PenaltyExcessiveRate;
            reasons.Add("Gewünschte Rate übersteigt das tragbare Maß (mehr als 70% des verfügbaren Einkommens)");
        }

        if (employmentStatus == EmploymentStatus.Employed)
        {
            reasons.Add("Angestelltenverhältnis bietet stabile Einkommenssituation");
        }
        else if (employmentStatus == EmploymentStatus.SelfEmployed)
        {
            score -= config.PenaltySelfEmployed;
            reasons.Add("Selbstständigkeit birgt gewisses Einkommensrisiko");
        }
        else if (employmentStatus == EmploymentStatus.Retired)
        {
            score -= config.PenaltyRetired;
            reasons.Add("Ruhestand bietet stabile, aber begrenzte Einkommenssituation");
        }
        else if (employmentStatus == EmploymentStatus.Unemployed)
        {
            score -= config.PenaltyUnemployed;
            reasons.Add("Arbeitslosigkeit stellt erhebliches Risiko für Kreditrückzahlung dar");
        }

        if (hasPaymentDefault)
        {
            score -= config.PenaltyPaymentDefault;
            reasons.Add("Frühere Zahlungsverzüge beeinträchtigen die Kreditwürdigkeit");
        }
        else
        {
            reasons.Add("Keine früheren Zahlungsverzüge - positive Zahlungshistorie");
        }

        if (creditScore.HasValue)
        {
            if (creditScore.Value >= config.CreditScoreGood)
            {
                reasons.Add("Guter externer Bonitätsscore");
            }
            else if (creditScore.Value >= config.CreditScoreModerate)
            {
                score -= config.PenaltyModerateCreditScore;
                reasons.Add("Moderater externer Bonitätsscore");
            }
            else
            {
                score -= config.PenaltyLowCreditScore;
                reasons.Add("Niedriger externer Bonitätsscore beeinträchtigt die Bewertung");
            }
        }
        else
        {
            reasons.Add("Kein externer Bonitätsscore verfügbar");
        }

        if (loanAmount is not null && loanTerm.HasValue)
        {
            var loanAmountValue = (double)loanAmount.Amount;
            var annualIncome = incomeAmount * 12;
            if (annualIncome > 0)
            {
                var loanToIncomeRatio = (decimal)(loanAmountValue / annualIncome);

                if (loanToIncomeRatio <= config.LoanToIncomeRatioGood)
                {
                    reasons.Add("Kreditbetrag steht in gutem Verhältnis zum Jahreseinkommen");
                }
                else if (loanToIncomeRatio <= config.LoanToIncomeRatioModerate)
                {
                    score -= config.PenaltyModerateLoanToIncome;
                    reasons.Add("Kreditbetrag ist moderat im Verhältnis zum Jahreseinkommen");
                }
                else if (loanToIncomeRatio <= config.LoanToIncomeRatioHigh)
                {
                    score -= config.PenaltyHighLoanToIncome;
                    reasons.Add("Kreditbetrag ist hoch im Verhältnis zum Jahreseinkommen");
                }
                else
                {
                    score -= config.PenaltyCriticalLoanToIncome;
                    reasons.Add("Kreditbetrag übersteigt das tragbare Maß im Verhältnis zum Jahreseinkommen");
                }
            }

            var loanTermValue = loanTerm.Value;
            if (loanTermValue <= config.LoanTermShort)
            {
                reasons.Add("Kurze Laufzeit reduziert das Risiko");
            }
            else if (loanTermValue <= config.LoanTermMedium)
            {
                score -= config.PenaltyMediumLoanTerm;
                reasons.Add("Mittlere Laufzeit birgt moderates Risiko");
            }
            else if (loanTermValue <= config.LoanTermLong)
            {
                score -= config.PenaltyLongLoanTerm;
                reasons.Add("Lange Laufzeit erhöht das Risiko");
            }
            else
            {
                score -= config.PenaltyVeryLongLoanTerm;
                reasons.Add("Sehr lange Laufzeit stellt erhebliches Risiko dar");
            }
        }

        score = Math.Max(0, Math.Min(100, score));

        TrafficLight trafficLight;
        if (score >= config.GreenThreshold)
        {
            trafficLight = TrafficLight.Green;
            reasons.Insert(0, "Gesamtbewertung: Positiv - Kreditantrag empfohlen");
        }
        else if (score >= config.YellowThreshold)
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
