import { dev } from '$app/environment';
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import type { Cookies } from '@sveltejs/kit';

const mockInsert = vi.fn().mockReturnValue({ values: vi.fn().mockResolvedValue(undefined) });
const mockSelectResult: Record<string, unknown>[] = [];
const mockSelect = vi.fn().mockReturnValue({
	from: vi.fn().mockReturnValue({
		where: vi.fn().mockImplementation(() => Promise.resolve([...mockSelectResult]))
	})
});
const mockDeleteResult = vi.fn().mockResolvedValue(undefined);
const mockDelete = vi.fn().mockReturnValue({
	where: mockDeleteResult,
	then: (resolve: (v: unknown) => void) => resolve(undefined)
});

vi.mock('$lib/server/db', () => ({
	db: {
		insert: (...args: unknown[]) => mockInsert(...args),
		select: (...args: unknown[]) => mockSelect(...args),
		delete: (...args: unknown[]) => mockDelete(...args)
	}
}));

vi.mock('$lib/server/db/schema', () => ({
	sessions: { id: 'id' }
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
		mockSelectResult.length = 0;
	});

	afterEach(() => {
		vi.restoreAllMocks();
	});

	it('creates a session and inserts into db', async () => {
		const cookies = createCookiesMock();

		const sessionId = await createSession(cookies, mockUser);

		expect(sessionId).toBeDefined();
		expect(mockInsert).toHaveBeenCalled();
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
		mockSelectResult.push({
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
		expect(mockDelete).toHaveBeenCalled();
	});

	it('returns user for valid session', async () => {
		const now = Date.now();
		mockSelectResult.push({
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

		expect(mockDelete).toHaveBeenCalled();
		expect(cookies.delete).toHaveBeenCalledWith(SESSION_COOKIE_NAME, { path: '/' });
	});

	it('clears all sessions', async () => {
		await clearSessions();

		expect(mockDelete).toHaveBeenCalled();
	});
});
