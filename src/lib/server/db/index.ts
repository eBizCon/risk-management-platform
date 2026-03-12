import { drizzle } from 'drizzle-orm/node-postgres';
import pg from 'pg';
import { env } from '$env/dynamic/private';
import * as schema from './schema';

const pool = new pg.Pool({
	connectionString: env.DATABASE_URL
});

export const db = drizzle(pool, { schema });

export { applicationInquiries, applications, sessions } from './schema';
export type {
	Application,
	ApplicationInquiry,
	ApplicationInquiryStatus,
	NewApplication,
	NewApplicationInquiry,
	ApplicationStatus,
	EmploymentStatus,
	TrafficLight,
	Session,
	NewSession
} from './schema';
