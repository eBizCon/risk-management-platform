import type { PageServerLoad } from './$types';
import { error } from '@sveltejs/kit';
import { getApplicationsByUser } from '$lib/server/services/repositories/application.repository';

export const load: PageServerLoad = async ({ url, locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	const statusFilter = url.searchParams.get('status');

	const applications = await getApplicationsByUser(
		locals.user.id,
		statusFilter as 'draft' | 'submitted' | 'approved' | 'rejected' | undefined
	);

	return {
		applications,
		statusFilter
	};
};
