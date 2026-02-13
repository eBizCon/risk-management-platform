import type { EmploymentStatus, TrafficLight } from '../db/schema';
import type { ParsedScoringConfig } from './repositories/scoring-config.repository';
import { getActiveScoringConfig } from './repositories/scoring-config.repository';

export interface ScoringResult {
	score: number;
	trafficLight: TrafficLight;
	reasons: string[];
}

export function calculateScoreWithConfig(
	income: number,
	fixedCosts: number,
	desiredRate: number,
	employmentStatus: EmploymentStatus,
	hasPaymentDefault: boolean,
	config: ParsedScoringConfig
): ScoringResult {
	const reasons: string[] = [];
	let score = 100;

	const availableIncome = income - fixedCosts;
	const incomeRatio = availableIncome / income;

	const irt = config.incomeRatioThresholds;
	if (incomeRatio >= irt.excellent) {
		reasons.push('Gutes Verhältnis zwischen Einkommen und Fixkosten (mehr als 50% verfügbar)');
	} else if (incomeRatio >= irt.good) {
		score -= 15;
		reasons.push('Moderates Verhältnis zwischen Einkommen und Fixkosten (30-50% verfügbar)');
	} else if (incomeRatio >= irt.moderate) {
		score -= 30;
		reasons.push('Eingeschränktes Verhältnis zwischen Einkommen und Fixkosten (10-30% verfügbar)');
	} else {
		score -= 50;
		reasons.push('Kritisches Verhältnis zwischen Einkommen und Fixkosten (weniger als 10% verfügbar)');
	}

	const rateToAvailableRatio = desiredRate / availableIncome;

	const at = config.affordabilityThresholds;
	if (rateToAvailableRatio <= at.comfortable) {
		reasons.push('Gewünschte Rate ist gut tragbar (maximal 30% des verfügbaren Einkommens)');
	} else if (rateToAvailableRatio <= at.moderate) {
		score -= 10;
		reasons.push('Gewünschte Rate ist moderat tragbar (30-50% des verfügbaren Einkommens)');
	} else if (rateToAvailableRatio <= at.stretched) {
		score -= 25;
		reasons.push('Gewünschte Rate belastet das Budget erheblich (50-70% des verfügbaren Einkommens)');
	} else {
		score -= 40;
		reasons.push('Gewünschte Rate übersteigt das tragbare Maß (mehr als 70% des verfügbaren Einkommens)');
	}

	const ed = config.employmentDeductions;
	switch (employmentStatus) {
		case 'employed':
			score -= ed.employed;
			reasons.push('Angestelltenverhältnis bietet stabile Einkommenssituation');
			break;
		case 'self_employed':
			score -= ed.self_employed;
			reasons.push('Selbstständigkeit birgt gewisses Einkommensrisiko');
			break;
		case 'retired':
			score -= ed.retired;
			reasons.push('Ruhestand bietet stabile, aber begrenzte Einkommenssituation');
			break;
		case 'unemployed':
			score -= ed.unemployed;
			reasons.push('Arbeitslosigkeit stellt erhebliches Risiko für Kreditrückzahlung dar');
			break;
	}

	if (hasPaymentDefault) {
		score -= config.paymentDefaultDeduction;
		reasons.push('Frühere Zahlungsverzüge beeinträchtigen die Kreditwürdigkeit');
	} else {
		reasons.push('Keine früheren Zahlungsverzüge - positive Zahlungshistorie');
	}

	score = Math.max(0, Math.min(100, score));

	let trafficLight: TrafficLight;
	if (score >= config.trafficLightGreen) {
		trafficLight = 'green';
		reasons.unshift('Gesamtbewertung: Positiv - Kreditantrag empfohlen');
	} else if (score >= config.trafficLightYellow) {
		trafficLight = 'yellow';
		reasons.unshift('Gesamtbewertung: Prüfung erforderlich - manuelle Bewertung empfohlen');
	} else {
		trafficLight = 'red';
		reasons.unshift('Gesamtbewertung: Kritisch - erhöhtes Risiko');
	}

	return { score, trafficLight, reasons };
}

export async function calculateScore(
	income: number,
	fixedCosts: number,
	desiredRate: number,
	employmentStatus: EmploymentStatus,
	hasPaymentDefault: boolean
): Promise<ScoringResult> {
	const config = await getActiveScoringConfig();
	return calculateScoreWithConfig(income, fixedCosts, desiredRate, employmentStatus, hasPaymentDefault, config);
}
