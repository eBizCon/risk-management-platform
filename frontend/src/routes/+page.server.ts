import type { PageServerLoad } from './$types';
import { getDashboardStats } from '$lib/server/services/repositories/application.repository';

export const load: PageServerLoad = async ({ parent }) => {
	const { user } = await parent();

	if (!user || (user.role !== 'applicant' && user.role !== 'processor')) {
		return { dashboardStats: null };
	}

	const userEmail = user.role === 'applicant' ? user.email : undefined;
	const dashboardStats = await getDashboardStats(userEmail);

	return { dashboardStats };
};
