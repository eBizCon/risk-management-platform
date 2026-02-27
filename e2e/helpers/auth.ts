import type { BrowserContext, Page } from '@playwright/test';

export type TestUserRole = 'applicant' | 'processor' | 'admin';

interface TestUser {
	id: string;
	email: string;
	name: string;
	role: TestUserRole;
}

const baseURL = process.env.PLAYWRIGHT_BASE_URL ?? 'http://localhost:4173';

type Cookie = Parameters<BrowserContext['addCookies']>[0][number];

const parseCookie = (setCookie: string, origin: URL): Cookie => {
	const [nameValue, ...rest] = setCookie.split(';').map((part) => part.trim());
	const [name, value] = nameValue.split('=');
	const cookie: Cookie = {
		name,
		value,
		domain: origin.hostname,
		path: '/'
	};

	rest.forEach((segment) => {
		const [rawKey, rawVal] = segment.split('=');
		const key = rawKey.toLowerCase();
		const val = rawVal ?? '';

		if (key === 'path') {
			cookie.path = val;
		}

		if (key === 'secure') {
			cookie.secure = true;
		}

		if (key === 'httponly') {
			cookie.httpOnly = true;
		}

		if (key === 'samesite') {
			const normalized = val.toLowerCase();
			if (normalized === 'lax') {
				cookie.sameSite = 'Lax';
			} else if (normalized === 'strict') {
				cookie.sameSite = 'Strict';
			} else if (normalized === 'none') {
				cookie.sameSite = 'None';
			}
		}

		if (key === 'max-age') {
			const maxAgeSeconds = Number(val);
			if (!Number.isNaN(maxAgeSeconds)) {
				cookie.expires = Math.floor(Date.now() / 1000) + maxAgeSeconds;
			}
		}
	});

	return cookie;
};

const defaultEmails: Record<TestUserRole, string> = {
	applicant: 'applicant@example.com',
	processor: 'processor@example.com',
	admin: 'admin@example.com'
};

const buildUser = (role: TestUserRole, userData?: Partial<TestUser>): TestUser => ({
	id: userData?.id ?? crypto.randomUUID(),
	email: userData?.email ?? defaultEmails[role],
	name: userData?.name ?? `Test User ${role}`,
	role
});

export const createTestSession = async (
	page: Page,
	role: TestUserRole,
	userData?: Partial<TestUser>
): Promise<string> => {
	const user = buildUser(role, userData);
	const response = await page.request.post('/api/test/session', {
		data: user,
		headers: { 'Content-Type': 'application/json' }
	});

	if (!response.ok()) {
		const message = await response.text();
		throw new Error(`Failed to create test session (${response.status()}): ${message}`);
	}

	const sessionId = (await response.json())?.sessionId as string | undefined;

	if (!sessionId) {
		throw new Error('Session creation did not return a sessionId');
	}

	const setCookieHeader = response.headers()['set-cookie'];

	if (!setCookieHeader) {
		throw new Error('Session creation did not return a session cookie');
	}

	const cookie = parseCookie(setCookieHeader, new URL(baseURL));

	await page.context().addCookies([cookie]);

	return sessionId;
};

export const clearTestSessions = async (page: Page): Promise<void> => {
	const response = await page.request.delete('/api/test/session');

	if (!response.ok()) {
		const message = await response.text();
		throw new Error(`Failed to clear test sessions (${response.status()}): ${message}`);
	}

	await page.context().clearCookies();
};
