import type { Actions } from './$types';
import { dev } from '$app/environment';
import { redirect } from '@sveltejs/kit';
import { createAuthorizationRequest } from '$lib/server/services/auth/oidc';

const TEMP_COOKIE_MAX_AGE_SECONDS = 300;
const tempCookieOptions = {
  httpOnly: true,
  sameSite: 'lax' as const,
  path: '/',
  secure: !dev,
  maxAge: TEMP_COOKIE_MAX_AGE_SECONDS
};

export const actions: Actions = {
  default: async ({ cookies, url }) => {
    const { url: authorizationUrl, codeVerifier, state } = await createAuthorizationRequest();

    cookies.set('pkce_verifier', codeVerifier, tempCookieOptions);
    cookies.set('oidc_state', state, tempCookieOptions);

    const returnTo = url.searchParams.get('returnTo');
    if (returnTo && returnTo.startsWith('/')) {
      cookies.set('returnTo', returnTo, tempCookieOptions);
    }

    throw redirect(302, authorizationUrl);
  }
};
