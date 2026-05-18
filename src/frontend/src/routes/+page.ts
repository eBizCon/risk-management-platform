import type { PageLoad } from './$types';
import type { DashboardStats } from '$lib/types';

export const load: PageLoad = async ({ fetch, parent }) => {
	const { user } = await parent();

	if (!user || (user.role !== 'applicant' && user.role !== 'processor')) {
		return { dashboardStats: null };
	}

	try {
		const res = await fetch('/api/applications/dashboard-stats');
		if (!res.ok) return { dashboardStats: null };
		const stats: DashboardStats = await res.json();
		return { dashboardStats: stats };
	} catch {
		return { dashboardStats: null };
	}
};
