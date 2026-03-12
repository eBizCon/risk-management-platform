import { eq } from 'drizzle-orm';
import { db, sessions } from '../../db';
import type { Session, NewSession } from '../../db/schema';

export async function insertSession(data: NewSession): Promise<void> {
	await db.insert(sessions).values(data);
}

export async function findSessionById(id: string): Promise<Session | null> {
	const [result] = await db.select().from(sessions).where(eq(sessions.id, id));
	return result ?? null;
}

export async function deleteSessionById(id: string): Promise<void> {
	await db.delete(sessions).where(eq(sessions.id, id));
}

export async function deleteAllSessions(): Promise<void> {
	await db.delete(sessions);
}
