import { redirect, type RequestHandler } from '@sveltejs/kit';
import { buildLogoutUrl } from '$lib/server/services/auth/oidc';
import { getOidcEnv } from '$lib/server/services/auth/env';
import { deleteSession, getSession, SESSION_COOKIE_NAME } from '$lib/server/services/auth/session';

export const GET: RequestHandler = async ({ cookies }) => {
  const sessionId = cookies.get(SESSION_COOKIE_NAME);
  const sessionUser = getSession(sessionId);

  deleteSession(cookies, sessionId);

  if (sessionUser?.idToken) {
    const logoutUrl = await buildLogoutUrl(sessionUser.idToken);
    if (logoutUrl) {
      throw redirect(302, logoutUrl);
    }
  }

  const { postLogoutRedirectUri } = getOidcEnv();
  throw redirect(302, postLogoutRedirectUri ?? '/');
};
