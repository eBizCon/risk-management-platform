import { redirect } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import {
	getOidcConfig,
	getOidcEnv,
	generatePkce,
	generateState,
	generateNonce,
	buildAuthorizationUrl
} from '$lib/server/auth/oidc';

export const GET: RequestHandler = async ({ url, cookies }) => {
	const returnTo = url.searchParams.get('returnTo') ?? '/';
	const safeReturnTo = returnTo.startsWith('/') && !returnTo.startsWith('//') ? returnTo : '/';

	const config = await getOidcConfig();
	const { scope, redirectUri } = getOidcEnv();

	const { codeVerifier, codeChallenge } = generatePkce();
	const state = generateState();
	const nonce = generateNonce();

	const cookieOptions = {
		path: '/',
		httpOnly: true,
		sameSite: 'lax' as const,
		secure: false,
		maxAge: 300
	};

	cookies.set('oidc_code_verifier', codeVerifier, cookieOptions);
	cookies.set('oidc_state', state, cookieOptions);
	cookies.set('oidc_nonce', nonce, cookieOptions);
	cookies.set('oidc_return_to', safeReturnTo, cookieOptions);

	const authUrl = buildAuthorizationUrl(config, {
		redirect_uri: redirectUri,
		scope,
		code_challenge: await codeChallenge,
		code_challenge_method: 'S256',
		state,
		nonce
	});

	redirect(302, authUrl.toString());
};
