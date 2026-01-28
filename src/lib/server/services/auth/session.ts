import { dev } from '$app/environment';
import type { Cookies } from '@sveltejs/kit';

export type SessionRecord = {
  user: App.User;
  expiresAt: number;
};

export const SESSION_COOKIE_NAME = 'session';
export const SESSION_MAX_AGE_SECONDS = 60 * 60;

const store = new Map<string, SessionRecord>();

const isExpired = (record: SessionRecord) => record.expiresAt <= Date.now();

export const createSession = (cookies: Cookies, user: App.User): string => {
  const sessionId = crypto.randomUUID();
  const expiresAt = Date.now() + SESSION_MAX_AGE_SECONDS * 1000;

  store.set(sessionId, { user, expiresAt });

  cookies.set(SESSION_COOKIE_NAME, sessionId, {
    httpOnly: true,
    sameSite: 'lax',
    path: '/',
    secure: !dev,
    maxAge: SESSION_MAX_AGE_SECONDS
  });

  return sessionId;
};

export const getSession = (sessionId: string | undefined): App.User | null => {
  if (!sessionId) {
    return null;
  }

  const record = store.get(sessionId);

  if (!record) {
    return null;
  }

  if (isExpired(record)) {
    store.delete(sessionId);
    return null;
  }

  return record.user;
};

export const deleteSession = (cookies: Cookies, sessionId: string | undefined): void => {
  if (sessionId) {
    store.delete(sessionId);
  }

  cookies.delete(SESSION_COOKIE_NAME, { path: '/' });
};

export const clearSessions = (): void => {
  store.clear();
};
