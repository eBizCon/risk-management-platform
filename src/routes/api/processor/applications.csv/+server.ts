import type { RequestHandler } from './$types';
import { error } from '@sveltejs/kit';
import { z } from 'zod';
import type { ApplicationStatus } from '$lib/server/db/schema';
import { getProcessorApplicationsForExport } from '$lib/server/services/repositories/application.repository';
import { buildApplicationsCsv } from '$lib/server/services/exports/applicationCsvExport';

const querySchema = z.object({
	status: z.enum(['draft', 'submitted', 'approved', 'rejected'] as const).optional()
});

function normalizeQuery(url: URL): { status?: ApplicationStatus } {
	const raw = Object.fromEntries(url.searchParams);
	if (raw.status === '') {
		delete (raw as Record<string, string>).status;
	}
	return querySchema.parse(raw);
}

export const GET: RequestHandler = async ({ url, locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}
	if (locals.user.role !== 'processor') {
		throw error(403, 'Keine Berechtigung');
	}

	const query = normalizeQuery(url);
	const apps = await getProcessorApplicationsForExport({ status: query.status });
	const csv = buildApplicationsCsv(apps);

	const today = new Date().toISOString().slice(0, 10);
	return new Response(csv, {
		headers: {
			'content-type': 'text/csv; charset=utf-8',
			'content-disposition': `attachment; filename="antraege-processor-${today}.csv"`
		}
	});
};
