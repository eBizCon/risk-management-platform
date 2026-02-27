import { json, error } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { getScoringConfig, updateScoringConfig } from '$lib/server/services/repositories/scoring-config.repository';
import { scoringConfigSchema } from '$lib/server/services/validation';
import { ZodError } from 'zod';

export const GET: RequestHandler = async ({ locals }) => {
	if (locals.user?.role !== 'admin') {
		throw error(403, 'Keine Berechtigung');
	}

	const config = await getScoringConfig();
	return json(config);
};

export const PUT: RequestHandler = async ({ locals, request }) => {
	if (locals.user?.role !== 'admin') {
		throw error(403, 'Keine Berechtigung');
	}

	try {
		const body = await request.json();
		const data = scoringConfigSchema.parse(body);
		const config = await updateScoringConfig(data, locals.user.email);
		return json(config);
	} catch (err) {
		if (err instanceof ZodError) {
			return json(
				{ error: 'Validierungsfehler', issues: err.issues },
				{ status: 400 }
			);
		}
		throw err;
	}
};
