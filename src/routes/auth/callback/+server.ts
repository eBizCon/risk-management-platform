import { error, redirect, type RequestHandler } from '@sveltejs/kit';
import { exchangeAuthorizationCode } from '$lib/server/services/auth/oidc';
import { createSession } from '$lib/server/services/auth/session';

const deleteTempCookies = (cookies: import('@sveltejs/kit').Cookies) => {
	cookies.delete('pkce_verifier', { path: '/' });
	cookies.delete('oidc_state', { path: '/' });
	cookies.delete('returnTo', { path: '/' });
};

export const GET: RequestHandler = async ({ request, cookies }) => {
	const codeVerifier = cookies.get('pkce_verifier');
	const expectedState = cookies.get('oidc_state');

	if (!codeVerifier || !expectedState) {
		deleteTempCookies(cookies);
		throw error(400, 'Ungültige Login-Anfrage');
	}

	const { tokens, idToken, roles } = await exchangeAuthorizationCode(request, {
		codeVerifier,
		expectedState
	});

	if (!roles.length) {
		deleteTempCookies(cookies);
		throw error(403, 'Keine Berechtigung');
	}

	const claims = (tokens.claims?.() ?? {}) as Partial<{ sub: string; name: string; email: string }>;
	const user: App.User = {
		id: claims.sub ?? 'unknown',
		email: claims.email ?? '',
		name: claims.name ?? 'Unbekannter Nutzer',
		role: roles[0],
		idToken
	};

	createSession(cookies, user);

	const returnTo = cookies.get('returnTo');
	deleteTempCookies(cookies);

	if (returnTo && returnTo.startsWith('/')) {
		throw redirect(302, returnTo);
	}

	throw redirect(302, '/');
};
