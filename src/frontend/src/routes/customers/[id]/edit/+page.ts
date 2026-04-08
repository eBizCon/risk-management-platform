import type { PageLoad } from './$types';
import { handleApiResponse } from '$lib/api';

export const load: PageLoad = async ({ fetch, params, url }) => {
	const res = await fetch(`/api/customers/${params.id}`);
	const body = await handleApiResponse<{ customer?: unknown } | unknown>(
		res,
		url,
		'Fehler beim Laden des Kunden'
	);
	return {
		customer:
			'customer' in (body as Record<string, unknown>)
				? (body as Record<string, unknown>).customer
				: body
	};
};
