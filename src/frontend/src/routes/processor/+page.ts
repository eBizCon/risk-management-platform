import type { PageLoad } from './$types';
import type { Application, DashboardStats } from '$lib/types';
import { handleApiResponse } from '$lib/api';

interface ProcessorPageData {
	applications: Application[];
	stats: DashboardStats;
	statusFilter?: string | null;
	pagination?: {
		page: number;
		totalPages: number;
		totalItems: number;
	};
}

export const load: PageLoad = async ({ fetch, url }) => {
	const status = url.searchParams.get('status');
	const page = url.searchParams.get('page');

	const params = new URLSearchParams();
	if (status) params.set('status', status);
	if (page) params.set('page', page);

	const query = params.toString() ? `?${params.toString()}` : '';
	const res = await fetch(`/api/processor/applications${query}`);

	return handleApiResponse<ProcessorPageData>(res, url, 'Fehler beim Laden');
};
