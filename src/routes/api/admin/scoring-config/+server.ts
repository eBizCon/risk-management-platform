import { json, error } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { getScoringConfig, updateScoringConfig } from '$lib/server/services/repositories/scoring_config.repository';
import { z } from 'zod';

const updateSchema = z.object({
	thresholdGreen: z.number().int().min(0).max(100).optional(),
	thresholdYellow: z.number().int().min(0).max(100).optional(),
	weightIncome: z.number().min(0).max(1).optional(),
	weightEmployment: z.number().min(0).max(1).optional(),
	weightPaymentDefault: z.number().min(0).max(1).optional()
});

export const GET: RequestHandler = async ({ locals }) => {
	if (locals.user?.role !== 'admin') throw error(403, 'Keine Berechtigung');
	return json(await getScoringConfig());
};

export const PUT: RequestHandler = async ({ locals, request }) => {
	if (locals.user?.role !== 'admin') throw error(403, 'Keine Berechtigung');

	const body = await request.json();
	const parsed = updateSchema.safeParse(body);

	if (!parsed.success) {
		throw error(400, 'Ungültige Eingabedaten: ' + parsed.error.message);
	}

	const config = await updateScoringConfig(parsed.data, locals.user.email);
	return json(config);
};
