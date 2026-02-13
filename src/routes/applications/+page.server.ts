import type { PageServerLoad } from './$types';
import { error } from '@sveltejs/kit';
import { getApplicationsByUser } from '$lib/server/services/repositories/application.repository';
import type { ApplicationSortOrder } from '$lib/server/services/repositories/application.repository';

export const load: PageServerLoad = async ({ url, locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	const statusFilter = url.searchParams.get('status');

	const sortParam = url.searchParams.get('sort');
	const sort: ApplicationSortOrder = sortParam === 'createdAt_asc' ? 'createdAt_asc' : 'createdAt_desc';

	const applications = await getApplicationsByUser(
		locals.user.id,
		statusFilter as 'draft' | 'submitted' | 'approved' | 'rejected' | undefined,
		sort
	);

	return {
		applications,
		statusFilter,
		sort
	};
};
