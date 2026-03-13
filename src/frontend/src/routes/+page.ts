import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch, parent }) => {
	const { user } = await parent();

	if (!user || (user.role !== 'applicant' && user.role !== 'processor')) {
		return { dashboardStats: null };
	}

	try {
		const res = await fetch('/api/dashboard/stats');
		if (res.ok) {
			const dashboardStats = await res.json();
			return { dashboardStats };
		}
	} catch {
		// ignore fetch errors
	}

	return { dashboardStats: null };
};
