import { drizzle } from 'drizzle-orm/node-postgres';
import pg from 'pg';
import { describe, it, expect, beforeAll, afterAll, beforeEach } from 'vitest';
import { sql } from 'drizzle-orm';
import { applications } from '../../db/schema';
import { getDashboardStats } from './application.repository';

describe('getDashboardStats', () => {
	let pool: pg.Pool;
	let database: ReturnType<typeof drizzle>;

	beforeAll(async () => {
		pool = new pg.Pool({
			connectionString: process.env.DATABASE_URL
		});
		database = drizzle(pool);

		await database.execute(sql`
			CREATE TABLE IF NOT EXISTS applications (
				id SERIAL PRIMARY KEY,
				name TEXT NOT NULL,
				income DOUBLE PRECISION NOT NULL,
				fixed_costs DOUBLE PRECISION NOT NULL,
				desired_rate DOUBLE PRECISION NOT NULL,
				employment_status TEXT NOT NULL,
				has_payment_default BOOLEAN NOT NULL,
				status TEXT NOT NULL DEFAULT 'draft',
				score INTEGER,
				traffic_light TEXT,
				scoring_reasons TEXT,
				processor_comment TEXT,
				created_at TEXT NOT NULL,
				submitted_at TEXT,
				processed_at TEXT,
				created_by TEXT NOT NULL
			)
		`);
	});

	afterAll(async () => {
		await pool.end();
	});

	beforeEach(async () => {
		await database.delete(applications);
	});

	function makeApplication(overrides: Partial<typeof applications.$inferInsert> = {}) {
		return {
			name: 'Test',
			income: 4000,
			fixedCosts: 1500,
			desiredRate: 500,
			employmentStatus: 'employed' as const,
			hasPaymentDefault: false,
			status: 'draft' as const,
			createdAt: new Date().toISOString(),
			createdBy: 'applicant@example.com',
			...overrides
		};
	}

	it('returns all zeros for empty database', async () => {
		const stats = await getDashboardStats();
		expect(stats).toEqual({ draft: 0, submitted: 0, approved: 0, rejected: 0 });
	});

	it('counts draft applications', async () => {
		await database
			.insert(applications)
			.values([makeApplication({ status: 'draft' }), makeApplication({ status: 'draft' })]);

		const stats = await getDashboardStats();
		expect(stats.draft).toBe(2);
		expect(stats.submitted).toBe(0);
		expect(stats.approved).toBe(0);
		expect(stats.rejected).toBe(0);
	});

	it('groups submitted, needs_information, and resubmitted into submitted', async () => {
		await database
			.insert(applications)
			.values([
				makeApplication({ status: 'submitted' }),
				makeApplication({ status: 'needs_information' }),
				makeApplication({ status: 'resubmitted' })
			]);

		const stats = await getDashboardStats();
		expect(stats.submitted).toBe(3);
	});

	it('counts approved and rejected separately', async () => {
		await database
			.insert(applications)
			.values([
				makeApplication({ status: 'approved' }),
				makeApplication({ status: 'approved' }),
				makeApplication({ status: 'rejected' })
			]);

		const stats = await getDashboardStats();
		expect(stats.approved).toBe(2);
		expect(stats.rejected).toBe(1);
	});

	it('filters by user email when provided', async () => {
		await database
			.insert(applications)
			.values([
				makeApplication({ status: 'draft', createdBy: 'user-a@example.com' }),
				makeApplication({ status: 'draft', createdBy: 'user-a@example.com' }),
				makeApplication({ status: 'draft', createdBy: 'user-b@example.com' }),
				makeApplication({ status: 'approved', createdBy: 'user-a@example.com' }),
				makeApplication({ status: 'approved', createdBy: 'user-b@example.com' })
			]);

		const statsA = await getDashboardStats('user-a@example.com');
		expect(statsA.draft).toBe(2);
		expect(statsA.approved).toBe(1);

		const statsB = await getDashboardStats('user-b@example.com');
		expect(statsB.draft).toBe(1);
		expect(statsB.approved).toBe(1);
	});

	it('returns all applications when no email is provided', async () => {
		await database
			.insert(applications)
			.values([
				makeApplication({ status: 'draft', createdBy: 'user-a@example.com' }),
				makeApplication({ status: 'submitted', createdBy: 'user-b@example.com' }),
				makeApplication({ status: 'approved', createdBy: 'user-a@example.com' })
			]);

		const stats = await getDashboardStats();
		expect(stats.draft).toBe(1);
		expect(stats.submitted).toBe(1);
		expect(stats.approved).toBe(1);
		expect(stats.rejected).toBe(0);
	});
});
