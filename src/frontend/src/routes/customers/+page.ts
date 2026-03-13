import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
	const res = await fetch('/api/customers');
	if (!res.ok) {
		throw error(res.status, 'Fehler beim Laden der Kunden');
	}
	return { customers: await res.json() };
};
