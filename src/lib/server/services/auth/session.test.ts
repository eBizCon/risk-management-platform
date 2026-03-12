import { dev } from '$app/environment';
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import type { Cookies } from '@sveltejs/kit';

const mockInsertSession = vi.fn().mockResolvedValue(undefined);
const mockFindSessionById = vi.fn().mockResolvedValue(null);
const mockDeleteSessionById = vi.fn().mockResolvedValue(undefined);
const mockDeleteAllSessions = vi.fn().mockResolvedValue(undefined);

vi.mock('../repositories/session.repository', () => ({
	insertSession: (...args: unknown[]) => mockInsertSession(...args),
	findSessionById: (...args: unknown[]) => mockFindSessionById(...args),
	deleteSessionById: (...args: unknown[]) => mockDeleteSessionById(...args),
	deleteAllSessions: (...args: unknown[]) => mockDeleteAllSessions(...args)
}));

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
	email: 'test@example.com',
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
		vi.clearAllMocks();
	});

	afterEach(() => {
		vi.restoreAllMocks();
	});

	it('creates a session and inserts into db', async () => {
		const cookies = createCookiesMock();

		const sessionId = await createSession(cookies, mockUser);

		expect(sessionId).toBeDefined();
		expect(mockInsertSession).toHaveBeenCalled();
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

	it('returns null for missing session', async () => {
		expect(await getSession(undefined)).toBeNull();
		expect(await getSession('unknown')).toBeNull();
	});

	it('deletes expired sessions', async () => {
		const now = Date.now();
		mockFindSessionById.mockResolvedValueOnce({
			id: 'session-1',
			userId: 'user-1',
			userEmail: 'test@example.com',
			userName: 'Test User',
			userRole: 'applicant',
			userIdToken: null,
			expiresAt: now - 1000
		});

		const result = await getSession('session-1');

		expect(result).toBeNull();
		expect(mockDeleteSessionById).toHaveBeenCalledWith('session-1');
	});

	it('returns user for valid session', async () => {
		const now = Date.now();
		mockFindSessionById.mockResolvedValueOnce({
			id: 'session-1',
			userId: 'user-1',
			userEmail: 'test@example.com',
			userName: 'Test User',
			userRole: 'applicant',
			userIdToken: null,
			expiresAt: now + 60_000
		});

		const result = await getSession('session-1');

		expect(result).toEqual({
			id: 'user-1',
			email: 'test@example.com',
			name: 'Test User',
			role: 'applicant',
			idToken: undefined
		});
	});

	it('deletes session and cookie', async () => {
		const cookies = createCookiesMock();

		await deleteSession(cookies, 'session-1');

		expect(mockDeleteSessionById).toHaveBeenCalledWith('session-1');
		expect(cookies.delete).toHaveBeenCalledWith(SESSION_COOKIE_NAME, { path: '/' });
	});

	it('clears all sessions', async () => {
		await clearSessions();

		expect(mockDeleteAllSessions).toHaveBeenCalled();
	});
});
