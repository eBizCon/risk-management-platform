import { drizzle } from 'drizzle-orm/node-postgres';
import pg from 'pg';
import { describe, it, expect, beforeAll, afterAll, beforeEach } from 'vitest';
import { sql, eq, count } from 'drizzle-orm';
import { seedDatabase } from './seed';
import { applications } from './schema';
import { calculateScore } from '../services/scoring';

describe('database seed', () => {
	let pool: pg.Pool;
	let database: ReturnType<typeof drizzle>;

	beforeAll(async () => {
		const dbUrl = new URL(
			process.env.DATABASE_URL || 'postgresql://risk:risk@localhost:5432/risk_management_test'
		);
		const dbName = dbUrl.pathname.slice(1);

		const adminUrl = new URL(dbUrl.toString());
		adminUrl.pathname = '/postgres';
		const adminPool = new pg.Pool({ connectionString: adminUrl.toString() });
		try {
			await adminPool.query(`CREATE DATABASE "${dbName}"`);
		} catch (err: unknown) {
			if ((err as { code?: string }).code !== '42P04') throw err;
		} finally {
			await adminPool.end();
		}

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
		// Clear the table before each test
		await database.delete(applications);
	});

	it('seeds at least 30 applications into an empty database', async () => {
		await seedDatabase(database);
		const [result] = await database.select({ value: count() }).from(applications);
		expect(result.value).toBeGreaterThanOrEqual(30);
	});

	it('creates an even distribution across all statuses', async () => {
		await seedDatabase(database);
		const rows = await database
			.select({
				status: applications.status,
				count: count()
			})
			.from(applications)
			.groupBy(applications.status);

		const counts = new Map(rows.map((row) => [row.status, row.count]));
		expect(counts.get('draft')).toBeGreaterThanOrEqual(7);
		expect(counts.get('submitted')).toBeGreaterThanOrEqual(7);
		expect(counts.get('approved')).toBeGreaterThanOrEqual(7);
		expect(counts.get('rejected')).toBeGreaterThanOrEqual(7);
	});

	it('includes all traffic light levels', async () => {
		await seedDatabase(database);
		const rows = await database
			.select({
				trafficLight: applications.trafficLight,
				count: count()
			})
			.from(applications)
			.groupBy(applications.trafficLight);

		const counts = new Map(rows.map((row) => [row.trafficLight, row.count]));
		expect(counts.get('green')).toBeGreaterThanOrEqual(1);
		expect(counts.get('yellow')).toBeGreaterThanOrEqual(1);
		expect(counts.get('red')).toBeGreaterThanOrEqual(1);
	});

	it('assigns every seeded application to applicant user', async () => {
		await seedDatabase(database);
		const rows = await database
			.selectDistinct({ createdBy: applications.createdBy })
			.from(applications);
		expect(rows).toEqual([{ createdBy: 'applicant@example.com' }]);
	});

	it('adds processor comments for approved and rejected applications', async () => {
		await seedDatabase(database);
		const approvedRows = await database
			.select({
				status: applications.status,
				processorComment: applications.processorComment
			})
			.from(applications)
			.where(eq(applications.status, 'approved'));

		const rejectedRows = await database
			.select({
				status: applications.status,
				processorComment: applications.processorComment
			})
			.from(applications)
			.where(eq(applications.status, 'rejected'));

		const rows = [...approvedRows, ...rejectedRows];
		expect(rows.length).toBeGreaterThan(0);
		rows.forEach((row) => {
			expect(row.processorComment).toBeTruthy();
		});
	});

	it('sets submitted and processed timestamps consistently', async () => {
		await seedDatabase(database);
		const submittedRows = await database
			.select({
				status: applications.status,
				createdAt: applications.createdAt,
				submittedAt: applications.submittedAt,
				processedAt: applications.processedAt
			})
			.from(applications)
			.where(eq(applications.status, 'submitted'));

		const approvedRows = await database
			.select({
				status: applications.status,
				createdAt: applications.createdAt,
				submittedAt: applications.submittedAt,
				processedAt: applications.processedAt
			})
			.from(applications)
			.where(eq(applications.status, 'approved'));

		const rejectedRows = await database
			.select({
				status: applications.status,
				createdAt: applications.createdAt,
				submittedAt: applications.submittedAt,
				processedAt: applications.processedAt
			})
			.from(applications)
			.where(eq(applications.status, 'rejected'));

		const rows = [...submittedRows, ...approvedRows, ...rejectedRows];
		rows.forEach((row) => {
			expect(row.submittedAt).toBeTruthy();
			expect(new Date(row.submittedAt as string).getTime()).toBeGreaterThan(
				new Date(row.createdAt).getTime()
			);
			if (row.status === 'approved' || row.status === 'rejected') {
				expect(row.processedAt).toBeTruthy();
				expect(new Date(row.processedAt as string).getTime()).toBeGreaterThan(
					new Date(row.submittedAt as string).getTime()
				);
			}
		});
	});

	it('stores score and traffic light according to calculateScore', async () => {
		await seedDatabase(database);
		const rows = await database
			.select({
				income: applications.income,
				fixedCosts: applications.fixedCosts,
				desiredRate: applications.desiredRate,
				employmentStatus: applications.employmentStatus,
				hasPaymentDefault: applications.hasPaymentDefault,
				score: applications.score,
				trafficLight: applications.trafficLight
			})
			.from(applications);

		rows.forEach((row) => {
			const scoring = calculateScore(
				row.income,
				row.fixedCosts,
				row.desiredRate,
				row.employmentStatus,
				row.hasPaymentDefault
			);
			expect(row.score).toBe(scoring.score);
			expect(row.trafficLight).toBe(scoring.trafficLight);
		});
	});

	it('does not insert additional rows when data already exists', async () => {
		await database.insert(applications).values({
			name: 'Existing Applicant',
			income: 3000,
			fixedCosts: 1000,
			desiredRate: 500,
			employmentStatus: 'employed',
			hasPaymentDefault: false,
			status: 'draft',
			createdAt: new Date().toISOString(),
			createdBy: 'applicant@example.com'
		});

		await seedDatabase(database);

		const [result] = await database.select({ value: count() }).from(applications);
		expect(result.value).toBe(1);
	});
});
