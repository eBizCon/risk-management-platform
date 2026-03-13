import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch, params }) => {
	const res = await fetch(`/api/customers/${params.id}`);
	if (!res.ok) {
		throw error(res.status, 'Fehler beim Laden des Kunden');
	}
	return { customer: await res.json() };
};
