import type { PageLoad } from './$types';
import { handleApiResponse } from '$lib/api';

export const load: PageLoad = async ({ fetch, url }) => {
	const res = await fetch('/api/customers');
	const body = await handleApiResponse<{ customers?: unknown[] } | unknown[]>(
		res,
		url,
		'Fehler beim Laden der Kunden'
	);
	return { customers: Array.isArray(body) ? body : (body.customers ?? []) };
};
