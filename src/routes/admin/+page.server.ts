import type { PageServerLoad, Actions } from './$types';
import { fail, error } from '@sveltejs/kit';
import { getScoringConfig, updateScoringConfig } from '$lib/server/services/repositories/scoring-config.repository';
import { scoringConfigSchema } from '$lib/server/services/validation';
import { ZodError } from 'zod';

export const load: PageServerLoad = async ({ locals }) => {
	if (!locals.user || locals.user.role !== 'admin') {
		throw error(403, 'Keine Berechtigung');
	}

	const config = await getScoringConfig();
	return { config };
};

export const actions: Actions = {
	default: async ({ request, locals }) => {
		if (!locals.user || locals.user.role !== 'admin') {
			throw error(403, 'Keine Berechtigung');
		}

		const formData = await request.formData();
		const rawData = {
			greenThreshold: parseInt(formData.get('greenThreshold') as string, 10),
			yellowThreshold: parseInt(formData.get('yellowThreshold') as string, 10),
			employedBonus: parseInt(formData.get('employedBonus') as string, 10),
			selfEmployedBonus: parseInt(formData.get('selfEmployedBonus') as string, 10),
			unemployedPenalty: parseInt(formData.get('unemployedPenalty') as string, 10),
			paymentDefaultPenalty: parseInt(formData.get('paymentDefaultPenalty') as string, 10)
		};

		try {
			const validatedData = scoringConfigSchema.parse(rawData);
			const config = await updateScoringConfig(validatedData, locals.user.email);
			return { success: true, config };
		} catch (err) {
			if (err instanceof ZodError) {
				const errors: Record<string, string[]> = {};
				err.issues.forEach((issue) => {
					const path = issue.path.join('.');
					if (!errors[path]) {
						errors[path] = [];
					}
					errors[path].push(issue.message);
				});
				return fail(400, { errors, values: rawData });
			}
			throw err;
		}
	}
};
