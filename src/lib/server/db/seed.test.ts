import Database from 'better-sqlite3';
import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { seedDatabase } from './seed';
import { calculateScore } from '../services/scoring';

const CREATE_APPLICATIONS_TABLE_SQL = `
	CREATE TABLE IF NOT EXISTS applications (
		id INTEGER PRIMARY KEY AUTOINCREMENT,
		name TEXT NOT NULL,
		income REAL NOT NULL,
		fixed_costs REAL NOT NULL,
		desired_rate REAL NOT NULL,
		employment_status TEXT NOT NULL CHECK(employment_status IN ('employed', 'self_employed', 'unemployed', 'retired')),
		has_payment_default INTEGER NOT NULL,
		status TEXT NOT NULL DEFAULT 'draft' CHECK(status IN ('draft', 'submitted', 'approved', 'rejected')),
		score INTEGER,
		traffic_light TEXT CHECK(traffic_light IN ('red', 'yellow', 'green')),
		scoring_reasons TEXT,
		processor_comment TEXT,
		created_at TEXT NOT NULL,
		submitted_at TEXT,
		processed_at TEXT,
		created_by TEXT NOT NULL
	)
`;

interface GroupCountRow {
	status?: 'draft' | 'submitted' | 'approved' | 'rejected';
	traffic_light?: 'red' | 'yellow' | 'green';
	count: number;
}

interface SeedRow {
	income: number;
	fixed_costs: number;
	desired_rate: number;
	employment_status: 'employed' | 'self_employed' | 'unemployed' | 'retired';
	has_payment_default: number;
	score: number;
	traffic_light: 'red' | 'yellow' | 'green';
	status: 'draft' | 'submitted' | 'approved' | 'rejected';
	created_by: string;
	submitted_at: string | null;
	processed_at: string | null;
	processor_comment: string | null;
	created_at: string;
}

describe('database seed', () => {
	let sqlite: Database.Database;

	beforeEach(() => {
		sqlite = new Database(':memory:');
		sqlite.exec(CREATE_APPLICATIONS_TABLE_SQL);
	});

	afterEach(() => {
		sqlite.close();
	});

	it('seeds at least 30 applications into an empty database', () => {
		seedDatabase(sqlite);
		const row = sqlite.prepare('SELECT COUNT(*) as count FROM applications').get() as {
			count: number;
		};
		expect(row.count).toBeGreaterThanOrEqual(30);
	});

	it('creates an even distribution across all statuses', () => {
		seedDatabase(sqlite);
		const rows = sqlite
			.prepare('SELECT status, COUNT(*) as count FROM applications GROUP BY status')
			.all() as GroupCountRow[];

		const counts = new Map(rows.map((row) => [row.status, row.count]));
		expect(counts.get('draft')).toBeGreaterThanOrEqual(7);
		expect(counts.get('submitted')).toBeGreaterThanOrEqual(7);
		expect(counts.get('approved')).toBeGreaterThanOrEqual(7);
		expect(counts.get('rejected')).toBeGreaterThanOrEqual(7);
	});

	it('includes all traffic light levels', () => {
		seedDatabase(sqlite);
		const rows = sqlite
			.prepare('SELECT traffic_light, COUNT(*) as count FROM applications GROUP BY traffic_light')
			.all() as GroupCountRow[];

		const counts = new Map(rows.map((row) => [row.traffic_light, row.count]));
		expect(counts.get('green')).toBeGreaterThanOrEqual(1);
		expect(counts.get('yellow')).toBeGreaterThanOrEqual(1);
		expect(counts.get('red')).toBeGreaterThanOrEqual(1);
	});

	it('assigns every seeded application to applicant user', () => {
		seedDatabase(sqlite);
		const rows = sqlite.prepare('SELECT DISTINCT created_by FROM applications').all() as Array<{
			created_by: string;
		}>;
		expect(rows).toEqual([{ created_by: 'applicant@example.com' }]);
	});

	it('adds processor comments for approved and rejected applications', () => {
		seedDatabase(sqlite);
		const rows = sqlite
			.prepare(
				"SELECT status, processor_comment FROM applications WHERE status IN ('approved', 'rejected')"
			)
			.all() as Array<{ status: 'approved' | 'rejected'; processor_comment: string | null }>;

		expect(rows.length).toBeGreaterThan(0);
		rows.forEach((row) => {
			expect(row.processor_comment).toBeTruthy();
		});
	});

	it('sets submitted and processed timestamps consistently', () => {
		seedDatabase(sqlite);
		const rows = sqlite
			.prepare(
				"SELECT status, created_at, submitted_at, processed_at FROM applications WHERE status IN ('submitted', 'approved', 'rejected')"
			)
			.all() as SeedRow[];

		rows.forEach((row) => {
			expect(row.submitted_at).toBeTruthy();
			expect(new Date(row.submitted_at as string).getTime()).toBeGreaterThan(
				new Date(row.created_at).getTime()
			);
			if (row.status === 'approved' || row.status === 'rejected') {
				expect(row.processed_at).toBeTruthy();
				expect(new Date(row.processed_at as string).getTime()).toBeGreaterThan(
					new Date(row.submitted_at as string).getTime()
				);
			}
		});
	});

	it('stores score and traffic light according to calculateScore', () => {
		seedDatabase(sqlite);
		const rows = sqlite
			.prepare(
				'SELECT income, fixed_costs, desired_rate, employment_status, has_payment_default, score, traffic_light FROM applications'
			)
			.all() as SeedRow[];

		rows.forEach((row) => {
			const scoring = calculateScore(
				row.income,
				row.fixed_costs,
				row.desired_rate,
				row.employment_status,
				Boolean(row.has_payment_default)
			);
			expect(row.score).toBe(scoring.score);
			expect(row.traffic_light).toBe(scoring.trafficLight);
		});
	});

	it('does not insert additional rows when data already exists', () => {
		sqlite
			.prepare(
				"INSERT INTO applications (name, income, fixed_costs, desired_rate, employment_status, has_payment_default, status, created_at, created_by) VALUES ('Existing Applicant', 3000, 1000, 500, 'employed', 0, 'draft', ?, 'applicant@example.com')"
			)
			.run(new Date().toISOString());

		seedDatabase(sqlite);

		const row = sqlite.prepare('SELECT COUNT(*) as count FROM applications').get() as {
			count: number;
		};
		expect(row.count).toBe(1);
	});
});
