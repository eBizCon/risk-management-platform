import { eq } from 'drizzle-orm';
import { db, scoringConfig } from '../../db';
import type { ScoringConfig } from '../../db/schema';

const DEFAULT_VALUES = {
	greenThreshold: 75,
	yellowThreshold: 50,
	employedBonus: 0,
	selfEmployedBonus: 10,
	unemployedPenalty: 35,
	paymentDefaultPenalty: 25
};

export async function getScoringConfig(): Promise<ScoringConfig> {
	const existing = db.select().from(scoringConfig).get();

	if (existing) {
		return existing;
	}

	const result = db
		.insert(scoringConfig)
		.values({
			...DEFAULT_VALUES,
			updatedAt: new Date().toISOString(),
			updatedBy: 'system'
		})
		.returning()
		.get();

	return result;
}

export async function updateScoringConfig(
	data: Omit<ScoringConfig, 'id' | 'updatedAt' | 'updatedBy'>,
	updatedBy: string
): Promise<ScoringConfig> {
	const existing = db.select().from(scoringConfig).get();

	if (existing) {
		const result = db
			.update(scoringConfig)
			.set({
				...data,
				updatedAt: new Date().toISOString(),
				updatedBy
			})
			.where(eq(scoringConfig.id, existing.id))
			.returning()
			.get();

		return result;
	}

	const result = db
		.insert(scoringConfig)
		.values({
			...data,
			updatedAt: new Date().toISOString(),
			updatedBy
		})
		.returning()
		.get();

	return result;
}
