import Database from 'better-sqlite3';
import { drizzle } from 'drizzle-orm/better-sqlite3';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import * as schema from '../../db/schema';

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

let sqlite: Database.Database;
let testDb: ReturnType<typeof drizzle>;

vi.mock('../../db', () => ({
	get db() {
		return testDb;
	},
	get applications() {
		return schema.applications;
	}
}));

import { getProcessorApplicationsPaginated } from './application.repository';

function insertApplication(overrides: Partial<{
	name: string;
	income: number;
	fixedCosts: number;
	desiredRate: number;
	employmentStatus: string;
	hasPaymentDefault: number;
	status: string;
	score: number;
	trafficLight: string;
	createdBy: string;
	createdAt: string;
}> = {}) {
	const defaults = {
		name: 'Test Applicant',
		income: 4000,
		fixedCosts: 1500,
		desiredRate: 500,
		employmentStatus: 'employed',
		hasPaymentDefault: 0,
		status: 'submitted',
		score: 75,
		trafficLight: 'green',
		createdBy: 'test@example.com',
		createdAt: new Date().toISOString()
	};
	const data = { ...defaults, ...overrides };
	sqlite.prepare(`
		INSERT INTO applications (name, income, fixed_costs, desired_rate, employment_status, has_payment_default, status, score, traffic_light, created_by, created_at)
		VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
	`).run(
		data.name, data.income, data.fixedCosts, data.desiredRate,
		data.employmentStatus, data.hasPaymentDefault, data.status,
		data.score, data.trafficLight, data.createdBy, data.createdAt
	);
}

describe('getProcessorApplicationsPaginated', () => {
	beforeEach(() => {
		sqlite = new Database(':memory:');
		sqlite.exec(CREATE_APPLICATIONS_TABLE_SQL);
		testDb = drizzle(sqlite, { schema });
	});

	afterEach(() => {
		sqlite.close();
	});

	it('returns all applications when no trafficLight filter is set', async () => {
		insertApplication({ trafficLight: 'green' });
		insertApplication({ trafficLight: 'yellow' });
		insertApplication({ trafficLight: 'red' });

		const result = await getProcessorApplicationsPaginated({ page: 1, pageSize: 10 });

		expect(result.totalCount).toBe(3);
		expect(result.items).toHaveLength(3);
	});

	it('filters by single trafficLight value', async () => {
		insertApplication({ trafficLight: 'green' });
		insertApplication({ trafficLight: 'yellow' });
		insertApplication({ trafficLight: 'red' });

		const result = await getProcessorApplicationsPaginated({
			trafficLight: ['red'],
			page: 1,
			pageSize: 10
		});

		expect(result.totalCount).toBe(1);
		expect(result.items).toHaveLength(1);
		expect(result.items[0].trafficLight).toBe('red');
	});

	it('filters by multiple trafficLight values', async () => {
		insertApplication({ trafficLight: 'green' });
		insertApplication({ trafficLight: 'yellow' });
		insertApplication({ trafficLight: 'red' });

		const result = await getProcessorApplicationsPaginated({
			trafficLight: ['red', 'yellow'],
			page: 1,
			pageSize: 10
		});

		expect(result.totalCount).toBe(2);
		expect(result.items).toHaveLength(2);
		const lights = result.items.map((i) => i.trafficLight).sort();
		expect(lights).toEqual(['red', 'yellow']);
	});

	it('combines status and trafficLight filters with AND', async () => {
		insertApplication({ status: 'submitted', trafficLight: 'red' });
		insertApplication({ status: 'submitted', trafficLight: 'green' });
		insertApplication({ status: 'approved', trafficLight: 'red' });

		const result = await getProcessorApplicationsPaginated({
			status: 'submitted',
			trafficLight: ['red'],
			page: 1,
			pageSize: 10
		});

		expect(result.totalCount).toBe(1);
		expect(result.items).toHaveLength(1);
		expect(result.items[0].status).toBe('submitted');
		expect(result.items[0].trafficLight).toBe('red');
	});

	it('returns all applications when trafficLight is an empty array', async () => {
		insertApplication({ trafficLight: 'green' });
		insertApplication({ trafficLight: 'yellow' });
		insertApplication({ trafficLight: 'red' });

		const result = await getProcessorApplicationsPaginated({
			trafficLight: [],
			page: 1,
			pageSize: 10
		});

		expect(result.totalCount).toBe(3);
		expect(result.items).toHaveLength(3);
	});
});
