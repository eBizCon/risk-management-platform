import { drizzle } from 'drizzle-orm/node-postgres';
import pg from 'pg';
import { DATABASE_URL } from '$env/static/private';
import * as schema from './schema';

const pool = new pg.Pool({
	connectionString: DATABASE_URL
});

export const db = drizzle(pool, { schema });

export { applications, sessions } from './schema';
export type {
	Application,
	NewApplication,
	ApplicationStatus,
	EmploymentStatus,
	TrafficLight
} from './schema';
