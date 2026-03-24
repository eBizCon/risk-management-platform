import { redirect } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import {
	getOidcConfig,
	getOidcEnv,
	exchangeCodeForTokens,
	extractUserFromTokens
} from '$lib/server/auth/oidc';
import { createSessionCookie } from '$lib/server/auth/session';

export const GET: RequestHandler = async ({ url, cookies }) => {
	const state = url.searchParams.get('state');
	const storedState = cookies.get('oidc_state');
	const codeVerifier = cookies.get('oidc_code_verifier');
	const nonce = cookies.get('oidc_nonce');
	const returnTo = cookies.get('oidc_return_to') ?? '/';

	cookies.delete('oidc_code_verifier', { path: '/' });
	cookies.delete('oidc_state', { path: '/' });
	cookies.delete('oidc_nonce', { path: '/' });
	cookies.delete('oidc_return_to', { path: '/' });

	if (!state || !storedState || state !== storedState || !codeVerifier || !nonce) {
		redirect(302, '/login?error=invalid_state');
	}

	const config = await getOidcConfig();
	const { redirectUri, rolesClaimPath } = getOidcEnv();

	const callbackUrl = new URL(url.pathname + url.search, redirectUri);

	const tokens = await exchangeCodeForTokens(config, callbackUrl, codeVerifier, storedState, nonce);
	const user = extractUserFromTokens(tokens, rolesClaimPath);

	if (!user) {
		redirect(302, '/login?error=no_role');
	}

	createSessionCookie(cookies, user);

	redirect(302, returnTo);
};
