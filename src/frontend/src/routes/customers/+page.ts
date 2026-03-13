import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
	const res = await fetch('/api/customers');
	if (!res.ok) {
		throw error(res.status, 'Fehler beim Laden der Kunden');
	}
	const body = await res.json();
	return { customers: body.customers ?? body };
};
