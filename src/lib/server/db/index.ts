import Database from 'better-sqlite3';
import { drizzle } from 'drizzle-orm/better-sqlite3';
import * as schema from './schema';
import { seedDatabase } from './seed';

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
		created_by TEXT NOT NULL,
		customer_id INTEGER
	)
`);

// Migration: add customer_id column if it doesn't exist (for existing databases)
try {
	sqlite.exec(`ALTER TABLE applications ADD COLUMN customer_id INTEGER`);
} catch {
	// Column already exists, ignore
}

seedDatabase(sqlite);

export { applications } from './schema';
export type {
	Application,
	NewApplication,
	ApplicationStatus,
	EmploymentStatus,
	TrafficLight
} from './schema';
