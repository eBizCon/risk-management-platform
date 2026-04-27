import type { PageLoad } from './$types';
import type { DashboardStats } from '$lib/types';

export const load: PageLoad = async ({ fetch, parent }) => {
	const { user } = await parent();

	let dashboardStats: DashboardStats | null = null;

	if (user && (user.role === 'applicant' || user.role === 'processor')) {
		try {
			const res = await fetch('/api/applications/dashboard-stats');
			if (res.ok) {
				dashboardStats = await res.json();
			}
		} catch {
			// AC4: hide dashboard silently on error
		}
	}

	return { dashboardStats };
};
