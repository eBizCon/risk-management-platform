import 'dotenv/config';
import { drizzle } from 'drizzle-orm/node-postgres';
import pg from 'pg';
import { seedDatabase } from '../src/lib/server/db/seed';

async function main() {
	const pool = new pg.Pool({
		connectionString: process.env.DATABASE_URL
	});

	const db = drizzle(pool);

	try {
		await seedDatabase(db);
		console.log('Database seeded successfully.');
	} finally {
		await pool.end();
	}
}

main().catch((error) => {
	console.error('Seeding failed:', error);
	process.exit(1);
});
