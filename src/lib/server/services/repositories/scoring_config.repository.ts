import { eq } from 'drizzle-orm';
import { db, scoringConfig } from '../../db';
import type { ScoringConfig } from '../../db/schema';

export async function getScoringConfig(): Promise<ScoringConfig> {
	const existing = db.select().from(scoringConfig).where(eq(scoringConfig.id, 1)).get();
	if (existing) {
		return existing;
	}

	const now = new Date().toISOString();
	const result = db
		.insert(scoringConfig)
		.values({
			id: 1,
			thresholdGreen: 75,
			thresholdYellow: 50,
			weightIncome: 0.4,
			weightEmployment: 0.3,
			weightPaymentDefault: 0.3,
			updatedAt: now,
			updatedBy: 'system'
		})
		.returning()
		.get();

	return result;
}

export async function updateScoringConfig(
	data: Partial<Omit<ScoringConfig, 'id' | 'updatedAt' | 'updatedBy'>>,
	updatedBy: string
): Promise<ScoringConfig> {
	// Ensure config exists
	await getScoringConfig();

	const result = db
		.update(scoringConfig)
		.set({
			...data,
			updatedAt: new Date().toISOString(),
			updatedBy
		})
		.where(eq(scoringConfig.id, 1))
		.returning()
		.get();

	return result;
}
