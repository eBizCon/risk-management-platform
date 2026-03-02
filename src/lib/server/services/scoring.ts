import type { EmploymentStatus, TrafficLight } from '../db/schema';

export interface ScoringWeights {
	income: number;
	fixedCosts: number;
	employment: number;
	paymentDefault: number;
}

export interface ScoringConfig {
	greenThreshold: number;
	yellowThreshold: number;
	weights: ScoringWeights;
}

export const DEFAULT_SCORING_CONFIG: ScoringConfig = {
	greenThreshold: 75,
	yellowThreshold: 50,
	weights: {
		income: 1.0,
		fixedCosts: 1.0,
		employment: 1.0,
		paymentDefault: 1.0
	}
};

export interface ScoringResult {
	score: number;
	trafficLight: TrafficLight;
	reasons: string[];
}

export function calculateScore(
	income: number,
	fixedCosts: number,
	desiredRate: number,
	employmentStatus: EmploymentStatus,
	hasPaymentDefault: boolean,
	config: ScoringConfig = DEFAULT_SCORING_CONFIG
): ScoringResult {
	const reasons: string[] = [];
	let score = 100;

	const availableIncome = income - fixedCosts;
	const incomeRatio = availableIncome / income;

	if (incomeRatio >= 0.5) {
		reasons.push('Gutes Verhältnis zwischen Einkommen und Fixkosten (mehr als 50% verfügbar)');
	} else if (incomeRatio >= 0.3) {
		score -= Math.round(15 * config.weights.income);
		reasons.push('Moderates Verhältnis zwischen Einkommen und Fixkosten (30-50% verfügbar)');
	} else if (incomeRatio >= 0.1) {
		score -= Math.round(30 * config.weights.income);
		reasons.push('Eingeschränktes Verhältnis zwischen Einkommen und Fixkosten (10-30% verfügbar)');
	} else {
		score -= Math.round(50 * config.weights.income);
		reasons.push('Kritisches Verhältnis zwischen Einkommen und Fixkosten (weniger als 10% verfügbar)');
	}

	const rateToAvailableRatio = desiredRate / availableIncome;

	if (rateToAvailableRatio <= 0.3) {
		reasons.push('Gewünschte Rate ist gut tragbar (maximal 30% des verfügbaren Einkommens)');
	} else if (rateToAvailableRatio <= 0.5) {
		score -= Math.round(10 * config.weights.fixedCosts);
		reasons.push('Gewünschte Rate ist moderat tragbar (30-50% des verfügbaren Einkommens)');
	} else if (rateToAvailableRatio <= 0.7) {
		score -= Math.round(25 * config.weights.fixedCosts);
		reasons.push('Gewünschte Rate belastet das Budget erheblich (50-70% des verfügbaren Einkommens)');
	} else {
		score -= Math.round(40 * config.weights.fixedCosts);
		reasons.push('Gewünschte Rate übersteigt das tragbare Maß (mehr als 70% des verfügbaren Einkommens)');
	}

	switch (employmentStatus) {
		case 'employed':
			reasons.push('Angestelltenverhältnis bietet stabile Einkommenssituation');
			break;
		case 'self_employed':
			score -= Math.round(10 * config.weights.employment);
			reasons.push('Selbstständigkeit birgt gewisses Einkommensrisiko');
			break;
		case 'retired':
			score -= Math.round(5 * config.weights.employment);
			reasons.push('Ruhestand bietet stabile, aber begrenzte Einkommenssituation');
			break;
		case 'unemployed':
			score -= Math.round(35 * config.weights.employment);
			reasons.push('Arbeitslosigkeit stellt erhebliches Risiko für Kreditrückzahlung dar');
			break;
	}

	if (hasPaymentDefault) {
		score -= Math.round(25 * config.weights.paymentDefault);
		reasons.push('Frühere Zahlungsverzüge beeinträchtigen die Kreditwürdigkeit');
	} else {
		reasons.push('Keine früheren Zahlungsverzüge - positive Zahlungshistorie');
	}

	score = Math.max(0, Math.min(100, score));

	let trafficLight: TrafficLight;
	if (score >= config.greenThreshold) {
		trafficLight = 'green';
		reasons.unshift('Gesamtbewertung: Positiv - Kreditantrag empfohlen');
	} else if (score >= config.yellowThreshold) {
		trafficLight = 'yellow';
		reasons.unshift('Gesamtbewertung: Prüfung erforderlich - manuelle Bewertung empfohlen');
	} else {
		trafficLight = 'red';
		reasons.unshift('Gesamtbewertung: Kritisch - erhöhtes Risiko');
	}

	return { score, trafficLight, reasons };
}
