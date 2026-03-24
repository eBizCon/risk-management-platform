import type { PageLoad } from './$types';
import { handleApiResponse } from '$lib/api';

export const load: PageLoad = async ({ fetch, url }) => {
	const status = url.searchParams.get('status');
	const query = status ? `?status=${status}` : '';
	const res = await fetch(`/api/applications${query}`);
	const applications = await handleApiResponse<string[]>(res, url, 'Fehler beim Laden');
	return { applications, statusFilter: status };
};
