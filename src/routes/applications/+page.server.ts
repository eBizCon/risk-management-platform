import type { PageServerLoad } from './$types';
import { getApplicationsByUser } from '$lib/server/services/repository';

export const load: PageServerLoad = async ({ url, cookies }) => {
	const userId = cookies.get('userId') || 'applicant-1';
	const statusFilter = url.searchParams.get('status');
	
	const applications = await getApplicationsByUser(
		userId,
		statusFilter as 'draft' | 'submitted' | 'approved' | 'rejected' | undefined
	);
	
	return {
		applications,
		statusFilter
	};
};
