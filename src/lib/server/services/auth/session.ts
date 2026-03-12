import { dev } from '$app/environment';
import { eq } from 'drizzle-orm';
import type { Cookies } from '@sveltejs/kit';
import { db } from '$lib/server/db';
import { sessions } from '$lib/server/db/schema';

export const SESSION_COOKIE_NAME = 'session';
export const SESSION_MAX_AGE_SECONDS = 60 * 60;

export const createSession = async (cookies: Cookies, user: App.User): Promise<string> => {
  const sessionId = crypto.randomUUID();
  const expiresAt = Date.now() + SESSION_MAX_AGE_SECONDS * 1000;

  await db.insert(sessions).values({
    id: sessionId,
    userId: user.id,
    userEmail: user.email,
    userName: user.name,
    userRole: user.role,
    userIdToken: user.idToken ?? null,
    expiresAt
  });

  cookies.set(SESSION_COOKIE_NAME, sessionId, {
    httpOnly: true,
    sameSite: 'lax',
    path: '/',
    secure: !dev,
    maxAge: SESSION_MAX_AGE_SECONDS
  });

  return sessionId;
};

export const getSession = async (sessionId: string | undefined): Promise<App.User | null> => {
  if (!sessionId) {
    return null;
  }

  const [record] = await db.select().from(sessions).where(eq(sessions.id, sessionId));

  if (!record) {
    return null;
  }

  if (record.expiresAt <= Date.now()) {
    await db.delete(sessions).where(eq(sessions.id, sessionId));
    return null;
  }

  return {
    id: record.userId,
    email: record.userEmail,
    name: record.userName,
    role: record.userRole,
    idToken: record.userIdToken ?? undefined
  };
};

export const deleteSession = async (cookies: Cookies, sessionId: string | undefined): Promise<void> => {
  if (sessionId) {
    await db.delete(sessions).where(eq(sessions.id, sessionId));
  }

  cookies.delete(SESSION_COOKIE_NAME, { path: '/' });
};

export const clearSessions = async (): Promise<void> => {
  await db.delete(sessions);
};
