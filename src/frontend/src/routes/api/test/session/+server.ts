import { json } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { env } from '$env/dynamic/private';
import { createSessionCookie, clearSessionCookie } from '$lib/server/auth/session';

const ALLOWED_ROLES: App.UserRole[] = ['applicant', 'processor', 'risk_manager'];

function isTestMode(): boolean {
	return env.NODE_ENV !== 'production' || env.TEST === 'true';
}

export const POST: RequestHandler = async ({ request, cookies }) => {
	if (!isTestMode()) {
		return json({ error: 'Not found' }, { status: 404 });
	}

	const body = await request.json();
	const { id, email, name, role } = body as {
		id?: string;
		email?: string;
		name?: string;
		role?: string;
	};

	if (!role || !ALLOWED_ROLES.includes(role as App.UserRole)) {
		return json({ error: 'Invalid role' }, { status: 400 });
	}
	if (!id) {
		return json({ error: 'Invalid id' }, { status: 400 });
	}
	if (!name) {
		return json({ error: 'Invalid name' }, { status: 400 });
	}

	const defaultEmails: Record<string, string> = {
		applicant: 'applicant@example.com',
		processor: 'processor@example.com'
	};

	const user: App.User = {
		id,
		email: email ?? defaultEmails[role] ?? `${role}@example.com`,
		name,
		role: role as App.UserRole
	};

	createSessionCookie(cookies, user);

	return json({ sessionId: 'cookie-auth' });
};

export const DELETE: RequestHandler = async ({ cookies }) => {
	if (!isTestMode()) {
		return json({ error: 'Not found' }, { status: 404 });
	}

	clearSessionCookie(cookies);

	return new Response(null, { status: 204 });
};
