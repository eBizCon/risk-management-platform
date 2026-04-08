import type { PageLoad } from './$types';
import type { Customer } from '$lib/types';
import { handleApiResponse } from '$lib/api';

export const load: PageLoad = async ({ fetch, params, url }) => {
	const res = await fetch(`/api/customers/${params.id}`);
	const body = await handleApiResponse<{ customer?: Customer } | Customer>(
		res,
		url,
		'Fehler beim Laden des Kunden'
	);
	return {
		customer:
			'customer' in (body as Record<string, unknown>)
				? (body as { customer: Customer }).customer
				: (body as Customer)
	};
};
