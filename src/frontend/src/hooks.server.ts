import type { Handle } from '@sveltejs/kit';
import { env } from '$env/dynamic/private';
import { readSessionCookie } from '$lib/server/auth/session';

const getRiskManagementApiUrl = (): string => env.RISK_MANAGEMENT_API_URL ?? 'http://localhost:5227';
const getCustomerServiceUrl = (): string => env.CUSTOMER_SERVICE_URL ?? 'http://localhost:5000';
const getServiceApiKey = (): string => env.SERVICE_API_KEY ?? '';

function getBackendUrl(pathname: string): string {
	if (pathname.startsWith('/api/customers')) {
		return getCustomerServiceUrl();
	}
	return getRiskManagementApiUrl();
}

export const handle: Handle = async ({ event, resolve }) => {
	const user = readSessionCookie(event.cookies);
	event.locals.user = user;

	if (event.url.pathname.startsWith('/api/test/')) {
		return resolve(event);
	}

	if (event.url.pathname.startsWith('/api/')) {
		if (!user) {
			return new Response(JSON.stringify({ error: 'Login erforderlich' }), {
				status: 401,
				headers: { 'Content-Type': 'application/json' }
			});
		}

		const backendUrl = getBackendUrl(event.url.pathname);
		const targetUrl = new URL(event.url.pathname + event.url.search, backendUrl);

		const headers = new Headers();
		headers.set('X-Api-Key', getServiceApiKey());

		if (user.accessToken) {
			headers.set('Authorization', `Bearer ${user.accessToken}`);
		} else {
			headers.set('X-User-Id', user.id);
			headers.set('X-User-Email', user.email);
			headers.set('X-User-Name', user.name);
			headers.set('X-User-Role', user.role);
		}

		const contentType = event.request.headers.get('content-type');
		if (contentType) {
			headers.set('Content-Type', contentType);
		}

		const accept = event.request.headers.get('accept');
		if (accept) {
			headers.set('Accept', accept);
		}

		const body = event.request.method !== 'GET' && event.request.method !== 'HEAD'
			? await event.request.arrayBuffer()
			: undefined;

		const response = await fetch(targetUrl.toString(), {
			method: event.request.method,
			headers,
			body
		});

		return new Response(response.body, {
			status: response.status,
			statusText: response.statusText,
			headers: {
				'Content-Type': response.headers.get('Content-Type') ?? 'application/json'
			}
		});
	}

	return resolve(event);
};
