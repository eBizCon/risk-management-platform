import Database from 'better-sqlite3';
import { drizzle } from 'drizzle-orm/better-sqlite3';
import * as schema from './schema';

const sqlite = new Database('data.db');
export const db = drizzle(sqlite, { schema });

sqlite.exec(`
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
`);

sqlite.exec(`
	CREATE TABLE IF NOT EXISTS scoring_config (
		id INTEGER PRIMARY KEY,
		traffic_light_green INTEGER NOT NULL,
		traffic_light_yellow INTEGER NOT NULL,
		income_ratio_thresholds TEXT NOT NULL,
		affordability_thresholds TEXT NOT NULL,
		employment_deductions TEXT NOT NULL,
		payment_default_deduction INTEGER NOT NULL,
		created_at INTEGER NOT NULL,
		updated_at INTEGER NOT NULL
	)
`);

const existingConfig = sqlite.prepare('SELECT id FROM scoring_config WHERE id = 1').get();
if (!existingConfig) {
	const now = Math.floor(Date.now() / 1000);
	sqlite.prepare(`
		INSERT INTO scoring_config (id, traffic_light_green, traffic_light_yellow, income_ratio_thresholds, affordability_thresholds, employment_deductions, payment_default_deduction, created_at, updated_at)
		VALUES (1, 75, 50, ?, ?, ?, 25, ?, ?)
	`).run(
		JSON.stringify({ excellent: 0.5, good: 0.3, moderate: 0.1 }),
		JSON.stringify({ comfortable: 0.3, moderate: 0.5, stretched: 0.7 }),
		JSON.stringify({ employed: 0, self_employed: 10, retired: 5, unemployed: 35 }),
		now,
		now
	);
}

export { applications, scoringConfig } from './schema';
export type { Application, NewApplication, ApplicationStatus, EmploymentStatus, TrafficLight, ScoringConfig, NewScoringConfig } from './schema';
