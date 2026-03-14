import { redirect } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { getOidcConfig, getEndSessionUrl } from '$lib/server/auth/oidc';
import { readSessionCookie, clearSessionCookie } from '$lib/server/auth/session';

export const GET: RequestHandler = async ({ cookies }) => {
	const user = readSessionCookie(cookies);
	clearSessionCookie(cookies);

	const config = await getOidcConfig();
	const endSessionUrl = getEndSessionUrl(config, user?.idToken);

	redirect(302, endSessionUrl);
};
