import { desc } from 'drizzle-orm';
import { db, scoringConfig } from '../../db';
import type { ScoringConfig, ScoringWeights } from '../scoring';
import { DEFAULT_SCORING_CONFIG } from '../scoring';

export async function getActiveScoringConfig(): Promise<ScoringConfig> {
	const row = db
		.select()
		.from(scoringConfig)
		.orderBy(desc(scoringConfig.createdAt))
		.limit(1)
		.get();

	if (!row) {
		return DEFAULT_SCORING_CONFIG;
	}

	const weights: ScoringWeights = JSON.parse(row.weights) as ScoringWeights;

	return {
		greenThreshold: row.greenThreshold,
		yellowThreshold: row.yellowThreshold,
		weights
	};
}

export async function saveScoringConfig(
	config: {
		greenThreshold: number;
		yellowThreshold: number;
		weights: ScoringWeights;
	},
	createdBy: string
): Promise<ScoringConfig> {
	const row = db
		.insert(scoringConfig)
		.values({
			greenThreshold: config.greenThreshold,
			yellowThreshold: config.yellowThreshold,
			weights: JSON.stringify(config.weights),
			createdAt: new Date(),
			createdBy
		})
		.returning()
		.get();

	const weights: ScoringWeights = JSON.parse(row.weights) as ScoringWeights;

	return {
		greenThreshold: row.greenThreshold,
		yellowThreshold: row.yellowThreshold,
		weights
	};
}

export async function getScoringConfigHistory(): Promise<ScoringConfig[]> {
	const rows = db
		.select()
		.from(scoringConfig)
		.orderBy(desc(scoringConfig.createdAt))
		.all();

	return rows.map((row) => ({
		greenThreshold: row.greenThreshold,
		yellowThreshold: row.yellowThreshold,
		weights: JSON.parse(row.weights) as ScoringWeights
	}));
}
