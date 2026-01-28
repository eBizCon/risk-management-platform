import { error, type Handle } from '@sveltejs/kit';
import { getSession, SESSION_COOKIE_NAME } from '$lib/server/services/auth/session';

const PUBLIC_PATHS = new Set(['/', '/login', '/auth/callback', '/logout', '/robots.txt', '/favicon.ico', '/api/test/session']);

const isPublicPath = (pathname: string): boolean => {
  if (PUBLIC_PATHS.has(pathname)) {
    return true;
  }

  return pathname.startsWith('/_app/') || pathname.startsWith('/assets/');
};

const requiresApplicant = (pathname: string): boolean => pathname.startsWith('/applications');
const requiresProcessor = (pathname: string): boolean => pathname.startsWith('/processor');

const respondAuthError = (status: 401 | 403, message: string, isApi: boolean): Response => {
  if (isApi) {
    return new Response(JSON.stringify({ error: message }), {
      status,
      headers: { 'content-type': 'application/json' }
    });
  }

  throw error(status, message);
};

export const handle: Handle = async ({ event, resolve }) => {
  const { pathname } = new URL(event.request.url);
  const isApiRoute = pathname.startsWith('/api/');

  const sessionId = event.cookies.get(SESSION_COOKIE_NAME);
  const user = getSession(sessionId);

  if (user) {
    event.locals.user = user;
  }

  if (isPublicPath(pathname)) {
    return resolve(event);
  }

  if (!event.locals.user) {
    return respondAuthError(401, 'Login erforderlich', isApiRoute);
  }

  const role = event.locals.user.role;

  if (requiresApplicant(pathname) && role !== 'applicant') {
    return respondAuthError(403, 'Keine Berechtigung', isApiRoute);
  }

  if (requiresProcessor(pathname) && role !== 'processor') {
    return respondAuthError(403, 'Keine Berechtigung', isApiRoute);
  }

  return resolve(event);
};
