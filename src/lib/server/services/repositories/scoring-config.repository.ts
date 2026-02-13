import { eq } from 'drizzle-orm';
import { db, scoringConfig } from '../../db';
import type { ScoringConfig } from '../../db/schema';

export interface IncomeRatioThresholds {
	excellent: number;
	good: number;
	moderate: number;
}

export interface AffordabilityThresholds {
	comfortable: number;
	moderate: number;
	stretched: number;
}

export interface EmploymentDeductions {
	employed: number;
	self_employed: number;
	retired: number;
	unemployed: number;
}

export interface ParsedScoringConfig {
	id: number;
	trafficLightGreen: number;
	trafficLightYellow: number;
	incomeRatioThresholds: IncomeRatioThresholds;
	affordabilityThresholds: AffordabilityThresholds;
	employmentDeductions: EmploymentDeductions;
	paymentDefaultDeduction: number;
	createdAt: Date;
	updatedAt: Date;
}

function parseScoringConfig(raw: ScoringConfig): ParsedScoringConfig {
	return {
		id: raw.id,
		trafficLightGreen: raw.trafficLightGreen,
		trafficLightYellow: raw.trafficLightYellow,
		incomeRatioThresholds: JSON.parse(raw.incomeRatioThresholds) as IncomeRatioThresholds,
		affordabilityThresholds: JSON.parse(raw.affordabilityThresholds) as AffordabilityThresholds,
		employmentDeductions: JSON.parse(raw.employmentDeductions) as EmploymentDeductions,
		paymentDefaultDeduction: raw.paymentDefaultDeduction,
		createdAt: raw.createdAt,
		updatedAt: raw.updatedAt
	};
}

export function getDefaultScoringConfig(): ParsedScoringConfig {
	return {
		id: 1,
		trafficLightGreen: 75,
		trafficLightYellow: 50,
		incomeRatioThresholds: { excellent: 0.5, good: 0.3, moderate: 0.1 },
		affordabilityThresholds: { comfortable: 0.3, moderate: 0.5, stretched: 0.7 },
		employmentDeductions: { employed: 0, self_employed: 10, retired: 5, unemployed: 35 },
		paymentDefaultDeduction: 25,
		createdAt: new Date(),
		updatedAt: new Date()
	};
}

export async function getActiveScoringConfig(): Promise<ParsedScoringConfig> {
	const raw = db.select().from(scoringConfig).where(eq(scoringConfig.id, 1)).get();
	if (!raw) {
		return getDefaultScoringConfig();
	}
	return parseScoringConfig(raw);
}

export async function updateScoringConfig(config: {
	trafficLightGreen: number;
	trafficLightYellow: number;
	incomeRatioThresholds: IncomeRatioThresholds;
	affordabilityThresholds: AffordabilityThresholds;
	employmentDeductions: EmploymentDeductions;
	paymentDefaultDeduction: number;
}): Promise<ParsedScoringConfig> {
	const now = new Date();
	const result = db
		.update(scoringConfig)
		.set({
			trafficLightGreen: config.trafficLightGreen,
			trafficLightYellow: config.trafficLightYellow,
			incomeRatioThresholds: JSON.stringify(config.incomeRatioThresholds),
			affordabilityThresholds: JSON.stringify(config.affordabilityThresholds),
			employmentDeductions: JSON.stringify(config.employmentDeductions),
			paymentDefaultDeduction: config.paymentDefaultDeduction,
			updatedAt: now
		})
		.where(eq(scoringConfig.id, 1))
		.returning()
		.get();

	if (!result) {
		throw new Error('Failed to update scoring configuration');
	}

	return parseScoringConfig(result);
}
