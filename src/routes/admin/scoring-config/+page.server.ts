import { redirect } from '@sveltejs/kit';
import type { PageServerLoad } from './$types';
import { getActiveScoringConfig } from '$lib/server/services/repositories/scoring-config.repository';

export const load: PageServerLoad = async ({ locals }) => {
	if (!locals.user || locals.user.role !== 'admin') {
		throw redirect(302, '/');
	}

	const config = await getActiveScoringConfig();

	return { config };
};
