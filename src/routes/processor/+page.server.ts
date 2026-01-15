import type { PageServerLoad } from './$types';
import { getAllApplications } from '$lib/server/services/repository';

export const load: PageServerLoad = async ({ url }) => {
	const statusFilter = url.searchParams.get('status');
	
	const allApplications = await getAllApplications();
	
	const applications = statusFilter
		? allApplications.filter(app => app.status === statusFilter)
		: allApplications;
	
	const stats = {
		total: allApplications.length,
		submitted: allApplications.filter(app => app.status === 'submitted').length,
		approved: allApplications.filter(app => app.status === 'approved').length,
		rejected: allApplications.filter(app => app.status === 'rejected').length
	};
	
	return {
		applications,
		statusFilter,
		stats
	};
};
