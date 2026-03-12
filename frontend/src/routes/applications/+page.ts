import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch, url }) => {
	const status = url.searchParams.get('status');
	const query = status ? `?status=${status}` : '';
	const res = await fetch(`/api/applications${query}`);
	if (!res.ok) {
		throw error(res.status, 'Fehler beim Laden');
	}
	return { applications: await res.json(), statusFilter: status };
};
