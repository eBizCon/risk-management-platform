import { json, error } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { getActiveScoringConfig, updateScoringConfig } from '$lib/server/services/repositories/scoring-config.repository';
import { scoringConfigSchema } from '$lib/server/services/validation';

export const GET: RequestHandler = async ({ locals }) => {
	if (!locals.user || locals.user.role !== 'admin') {
		throw error(403, 'Keine Berechtigung');
	}

	const config = await getActiveScoringConfig();
	return json(config);
};

export const PUT: RequestHandler = async ({ request, locals }) => {
	if (!locals.user || locals.user.role !== 'admin') {
		throw error(403, 'Keine Berechtigung');
	}

	let body: unknown;
	try {
		body = await request.json();
	} catch {
		throw error(400, 'Ungültiges JSON');
	}

	const parsed = scoringConfigSchema.safeParse(body);
	if (!parsed.success) {
		return json(
			{
				error: 'Validierungsfehler',
				details: parsed.error.issues.map((issue) => ({
					path: issue.path.join('.'),
					message: issue.message
				}))
			},
			{ status: 400 }
		);
	}

	const updated = await updateScoringConfig(parsed.data);
	return json(updated);
};
