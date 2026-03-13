import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch, url }) => {
	const status = url.searchParams.get('status');
	const page = url.searchParams.get('page');

	const params = new URLSearchParams();
	if (status) params.set('status', status);
	if (page) params.set('page', page);

	const query = params.toString() ? `?${params.toString()}` : '';
	const res = await fetch(`/api/processor/applications${query}`);

	if (!res.ok) {
		throw error(res.status, 'Fehler beim Laden');
	}

	return await res.json();
};
