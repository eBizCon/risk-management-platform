import type { PageServerLoad } from './$types';
import type { DashboardStats } from '$lib/types';

export const load: PageServerLoad = async ({ fetch, locals }) => {
	const user = locals.user;

	if (!user || (user.role !== 'applicant' && user.role !== 'processor')) {
		return { dashboardStats: null as DashboardStats | null };
	}

	try {
		const res = await fetch('/api/applications/dashboard-stats');
		if (!res.ok) return { dashboardStats: null as DashboardStats | null };
		const stats: DashboardStats = await res.json();
		return { dashboardStats: stats };
	} catch {
		return { dashboardStats: null as DashboardStats | null };
	}
};
