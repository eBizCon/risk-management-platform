import { dev } from '$app/environment';
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import type { Cookies } from '@sveltejs/kit';
import {
  SESSION_COOKIE_NAME,
  SESSION_MAX_AGE_SECONDS,
  clearSessions,
  createSession,
  deleteSession,
  getSession
} from './session';

const mockUser: App.User = {
  id: 'user-1',
  name: 'Test User',
  role: 'applicant'
};

const createCookiesMock = () => {
  const store = new Map<string, string>();

  return {
    set: vi.fn((name: string, value: string) => {
      store.set(name, value);
    }),
    get: vi.fn((name: string) => store.get(name)),
    delete: vi.fn((name: string) => {
      store.delete(name);
    })
  } as unknown as Cookies;
};

describe('session service', () => {
  beforeEach(() => {
    clearSessions();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('creates a session and sets cookie with secure based on env', () => {
    const cookies = createCookiesMock();

    const sessionId = createSession(cookies, mockUser);

    expect(sessionId).toBeDefined();
    expect(getSession(sessionId)).toEqual(mockUser);
    expect(cookies.set).toHaveBeenCalledWith(
      SESSION_COOKIE_NAME,
      sessionId,
      expect.objectContaining({
        httpOnly: true,
        sameSite: 'lax',
        path: '/',
        secure: !dev,
        maxAge: SESSION_MAX_AGE_SECONDS
      })
    );
  });

  it('returns null for missing session', () => {
    expect(getSession(undefined)).toBeNull();
    expect(getSession('unknown')).toBeNull();
  });

  it('deletes expired sessions', () => {
    const cookies = createCookiesMock();
    const sessionId = createSession(cookies, mockUser);

    const now = Date.now();
    vi.spyOn(Date, 'now').mockReturnValue(now + (SESSION_MAX_AGE_SECONDS + 1) * 1000);

    expect(getSession(sessionId)).toBeNull();
  });

  it('deletes session and cookie', () => {
    const cookies = createCookiesMock();
    const sessionId = createSession(cookies, mockUser);

    deleteSession(cookies, sessionId);

    expect(getSession(sessionId)).toBeNull();
    expect(cookies.delete).toHaveBeenCalledWith(SESSION_COOKIE_NAME, { path: '/' });
  });
});
