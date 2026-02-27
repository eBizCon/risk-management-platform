import { error } from '@sveltejs/kit';
import type { PageServerLoad } from './$types';
import { getScoringConfig } from '$lib/server/services/repositories/scoring_config.repository';

export const load: PageServerLoad = async ({ locals }) => {
	if (locals.user?.role !== 'admin') throw error(403, 'Keine Berechtigung');
	return { config: await getScoringConfig() };
};
