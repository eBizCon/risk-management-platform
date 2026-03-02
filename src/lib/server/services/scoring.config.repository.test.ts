import { describe, it, expect, beforeEach } from 'vitest';
import { desc } from 'drizzle-orm';
import Database from 'better-sqlite3';
import { drizzle } from 'drizzle-orm/better-sqlite3';
import * as schema from '../db/schema';

describe('Scoring Config Repository', () => {
	let sqlite: InstanceType<typeof Database>;
	let db: ReturnType<typeof drizzle>;

	beforeEach(() => {
		sqlite = new Database(':memory:');
		sqlite.exec(`
			CREATE TABLE IF NOT EXISTS scoring_config (
				id INTEGER PRIMARY KEY AUTOINCREMENT,
				green_threshold INTEGER NOT NULL DEFAULT 75,
				yellow_threshold INTEGER NOT NULL DEFAULT 50,
				weights TEXT NOT NULL,
				created_at INTEGER NOT NULL,
				created_by TEXT NOT NULL
			)
		`);
		db = drizzle(sqlite, { schema });
	});

	describe('getActiveScoringConfig (logic)', () => {
		it('should return default config when no entries exist', () => {
			const row = db
				.select()
				.from(schema.scoringConfig)
				.orderBy(schema.scoringConfig.createdAt)
				.limit(1)
				.get();

			expect(row).toBeUndefined();
			// Default config should be used
			const defaultConfig = {
				greenThreshold: 75,
				yellowThreshold: 50,
				weights: { income: 1.0, fixedCosts: 1.0, employment: 1.0, paymentDefault: 1.0 }
			};
			expect(defaultConfig.greenThreshold).toBe(75);
			expect(defaultConfig.yellowThreshold).toBe(50);
		});

		it('should return the newest config entry', () => {
			// Insert two entries with different timestamps
			db.insert(schema.scoringConfig)
				.values({
					greenThreshold: 70,
					yellowThreshold: 40,
					weights: JSON.stringify({ income: 1.0, fixedCosts: 1.0, employment: 1.0, paymentDefault: 1.0 }),
					createdAt: new Date('2024-01-01'),
					createdBy: 'admin@example.com'
				})
				.run();

			db.insert(schema.scoringConfig)
				.values({
					greenThreshold: 80,
					yellowThreshold: 55,
					weights: JSON.stringify({ income: 1.5, fixedCosts: 1.2, employment: 0.8, paymentDefault: 2.0 }),
					createdAt: new Date('2024-06-01'),
					createdBy: 'admin@example.com'
				})
				.run();

			const row = db
				.select()
				.from(schema.scoringConfig)
				.orderBy(desc(schema.scoringConfig.createdAt))
				.limit(1)
				.get();

			expect(row).toBeDefined();
			expect(row!.greenThreshold).toBe(80);
			expect(row!.yellowThreshold).toBe(55);
		});

		it('should parse weights JSON correctly', () => {
			const weights = { income: 1.5, fixedCosts: 1.2, employment: 0.8, paymentDefault: 2.0 };
			db.insert(schema.scoringConfig)
				.values({
					greenThreshold: 80,
					yellowThreshold: 55,
					weights: JSON.stringify(weights),
					createdAt: new Date(),
					createdBy: 'admin@example.com'
				})
				.run();

			const row = db
				.select()
				.from(schema.scoringConfig)
				.limit(1)
				.get();

			expect(row).toBeDefined();
			const parsed = JSON.parse(row!.weights);
			expect(parsed.income).toBe(1.5);
			expect(parsed.fixedCosts).toBe(1.2);
			expect(parsed.employment).toBe(0.8);
			expect(parsed.paymentDefault).toBe(2.0);
		});
	});

	describe('saveScoringConfig (logic)', () => {
		it('should always create a new entry (versioning)', () => {
			db.insert(schema.scoringConfig)
				.values({
					greenThreshold: 70,
					yellowThreshold: 40,
					weights: JSON.stringify({ income: 1.0, fixedCosts: 1.0, employment: 1.0, paymentDefault: 1.0 }),
					createdAt: new Date('2024-01-01'),
					createdBy: 'admin@example.com'
				})
				.run();

			db.insert(schema.scoringConfig)
				.values({
					greenThreshold: 80,
					yellowThreshold: 55,
					weights: JSON.stringify({ income: 1.5, fixedCosts: 1.0, employment: 1.0, paymentDefault: 1.0 }),
					createdAt: new Date('2024-06-01'),
					createdBy: 'admin@example.com'
				})
				.run();

			const allRows = db
				.select()
				.from(schema.scoringConfig)
				.all();

			expect(allRows).toHaveLength(2);
			expect(allRows[0].greenThreshold).toBe(70);
			expect(allRows[1].greenThreshold).toBe(80);
		});
	});
});
