import { json, error } from '@sveltejs/kit';
import { z } from 'zod';
import {
	getActiveScoringConfig,
	saveScoringConfig
} from '$lib/server/services/repositories/scoring.config.repository';
import type { RequestHandler } from './$types';

const scoringConfigSchema = z.object({
	greenThreshold: z.number().int().min(0).max(100),
	yellowThreshold: z.number().int().min(0).max(100),
	weights: z.object({
		income: z.number().min(0).max(5),
		fixedCosts: z.number().min(0).max(5),
		employment: z.number().min(0).max(5),
		paymentDefault: z.number().min(0).max(5)
	})
}).refine((data) => data.yellowThreshold < data.greenThreshold, {
	message: 'Der Gelb-Schwellenwert muss kleiner als der Grün-Schwellenwert sein'
});

export const GET: RequestHandler = async ({ locals }) => {
	if (!locals.user || locals.user.role !== 'admin') {
		throw error(403, 'Keine Berechtigung');
	}

	const config = await getActiveScoringConfig();
	return json(config);
};

export const POST: RequestHandler = async ({ request, locals }) => {
	if (!locals.user || locals.user.role !== 'admin') {
		throw error(403, 'Keine Berechtigung');
	}

	let body: unknown;

	try {
		body = await request.json();
	} catch {
		throw error(400, 'Invalid JSON payload');
	}

	const parsed = scoringConfigSchema.safeParse(body);

	if (!parsed.success) {
		throw error(400, parsed.error.issues.map((issue) => issue.message).join('; '));
	}

	const saved = await saveScoringConfig(parsed.data, locals.user.email);
	return json(saved, { status: 201 });
};
