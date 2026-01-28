import type { PageServerLoad } from './$types';
import { error } from '@sveltejs/kit';
import { getApplicationsByStatus } from '$lib/server/services/repositories/application.repository';
import type { ApplicationStatus } from '$lib/server/db/schema';

export const load: PageServerLoad = async ({ url, locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	if (locals.user.role !== 'processor') {
		throw error(403, 'Keine Berechtigung');
	}
	const statusFilter = url.searchParams.get('status') as ApplicationStatus | null;
	
	const allApplications = await getApplicationsByStatus(statusFilter || 'submitted');
	
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
