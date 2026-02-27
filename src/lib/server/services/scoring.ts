import type { EmploymentStatus, TrafficLight, ScoringConfig } from '../db/schema';

export interface ScoringResult {
	score: number;
	trafficLight: TrafficLight;
	reasons: string[];
}

export interface ScoringConfigValues {
	greenThreshold: number;
	yellowThreshold: number;
	employedBonus: number;
	selfEmployedBonus: number;
	unemployedPenalty: number;
	paymentDefaultPenalty: number;
}

const DEFAULT_CONFIG: ScoringConfigValues = {
	greenThreshold: 75,
	yellowThreshold: 50,
	employedBonus: 0,
	selfEmployedBonus: 10,
	unemployedPenalty: 35,
	paymentDefaultPenalty: 25
};

function toConfigValues(config: ScoringConfig): ScoringConfigValues {
	return {
		greenThreshold: config.greenThreshold,
		yellowThreshold: config.yellowThreshold,
		employedBonus: config.employedBonus,
		selfEmployedBonus: config.selfEmployedBonus,
		unemployedPenalty: config.unemployedPenalty,
		paymentDefaultPenalty: config.paymentDefaultPenalty
	};
}

export function calculateScore(
	income: number,
	fixedCosts: number,
	desiredRate: number,
	employmentStatus: EmploymentStatus,
	hasPaymentDefault: boolean,
	config: ScoringConfigValues | ScoringConfig = DEFAULT_CONFIG
): ScoringResult {
	const cfg: ScoringConfigValues = 'id' in config ? toConfigValues(config) : config;
	const reasons: string[] = [];
	let score = 100;

	const availableIncome = income - fixedCosts;
	const incomeRatio = availableIncome / income;

	if (incomeRatio >= 0.5) {
		reasons.push('Gutes Verhältnis zwischen Einkommen und Fixkosten (mehr als 50% verfügbar)');
	} else if (incomeRatio >= 0.3) {
		score -= 15;
		reasons.push('Moderates Verhältnis zwischen Einkommen und Fixkosten (30-50% verfügbar)');
	} else if (incomeRatio >= 0.1) {
		score -= 30;
		reasons.push('Eingeschränktes Verhältnis zwischen Einkommen und Fixkosten (10-30% verfügbar)');
	} else {
		score -= 50;
		reasons.push('Kritisches Verhältnis zwischen Einkommen und Fixkosten (weniger als 10% verfügbar)');
	}

	const rateToAvailableRatio = desiredRate / availableIncome;

	if (rateToAvailableRatio <= 0.3) {
		reasons.push('Gewünschte Rate ist gut tragbar (maximal 30% des verfügbaren Einkommens)');
	} else if (rateToAvailableRatio <= 0.5) {
		score -= 10;
		reasons.push('Gewünschte Rate ist moderat tragbar (30-50% des verfügbaren Einkommens)');
	} else if (rateToAvailableRatio <= 0.7) {
		score -= 25;
		reasons.push('Gewünschte Rate belastet das Budget erheblich (50-70% des verfügbaren Einkommens)');
	} else {
		score -= 40;
		reasons.push('Gewünschte Rate übersteigt das tragbare Maß (mehr als 70% des verfügbaren Einkommens)');
	}

	switch (employmentStatus) {
		case 'employed':
			reasons.push('Angestelltenverhältnis bietet stabile Einkommenssituation');
			break;
		case 'self_employed':
			score -= cfg.selfEmployedBonus;
			reasons.push('Selbstständigkeit birgt gewisses Einkommensrisiko');
			break;
		case 'retired':
			score -= 5;
			reasons.push('Ruhestand bietet stabile, aber begrenzte Einkommenssituation');
			break;
		case 'unemployed':
			score -= cfg.unemployedPenalty;
			reasons.push('Arbeitslosigkeit stellt erhebliches Risiko für Kreditrückzahlung dar');
			break;
	}

	if (hasPaymentDefault) {
		score -= cfg.paymentDefaultPenalty;
		reasons.push('Frühere Zahlungsverzüge beeinträchtigen die Kreditwürdigkeit');
	} else {
		reasons.push('Keine früheren Zahlungsverzüge - positive Zahlungshistorie');
	}

	score = Math.max(0, Math.min(100, score));

	let trafficLight: TrafficLight;
	if (score >= cfg.greenThreshold) {
		trafficLight = 'green';
		reasons.unshift('Gesamtbewertung: Positiv - Kreditantrag empfohlen');
	} else if (score >= cfg.yellowThreshold) {
		trafficLight = 'yellow';
		reasons.unshift('Gesamtbewertung: Prüfung erforderlich - manuelle Bewertung empfohlen');
	} else {
		trafficLight = 'red';
		reasons.unshift('Gesamtbewertung: Kritisch - erhöhtes Risiko');
	}

	return { score, trafficLight, reasons };
}
