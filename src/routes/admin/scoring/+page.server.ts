import { redirect } from '@sveltejs/kit';
import { getActiveScoringConfig } from '$lib/server/services/repositories/scoring.config.repository';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ locals }) => {
	if (!locals.user || locals.user.role !== 'admin') {
		throw redirect(303, '/');
	}

	const config = await getActiveScoringConfig();

	return { config };
};
