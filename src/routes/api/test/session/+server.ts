import { json, error } from '@sveltejs/kit';
import { dev } from '$app/environment';
import { env } from '$env/dynamic/private';
import { z } from 'zod';
import { createSession, deleteSession, SESSION_COOKIE_NAME } from '$lib/server/services/auth/session';
import type { RequestHandler } from './$types';

const requestSchema = z.object({
	role: z.enum(['applicant', 'processor', 'admin']),
	id: z.string().min(1),
	name: z.string().min(1)
});

const isTestMode = () => dev || env.TEST === 'true';

export const POST: RequestHandler = async ({ request, cookies }) => {
	if (!isTestMode()) {
		throw error(404, 'Not found');
	}

	let body: unknown;

	try {
		body = await request.json();
	} catch {
		throw error(400, 'Invalid JSON payload');
	}

	const parsed = requestSchema.safeParse(body);

	if (!parsed.success) {
		throw error(400, parsed.error.issues.map((issue) => issue.message).join('; '));
	}

	const { role, id, name } = parsed.data;

	const user: App.User = {
		id,
		name,
		role
	};

	const sessionId = createSession(cookies, user);

	return json({ sessionId });
};

export const DELETE: RequestHandler = async ({ cookies }) => {
	if (!isTestMode()) {
		throw error(404, 'Not found');
	}

	const sessionId = cookies.get(SESSION_COOKIE_NAME);

	deleteSession(cookies, sessionId);

	return new Response(null, { status: 204 });
};
