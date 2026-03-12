import type { RequestHandler } from './$types';
import { csvExportSchema } from '$lib/server/services/validation';
import { requestCsvExport } from '$lib/server/services/csv-export.service';

export const GET: RequestHandler = async ({ url, locals }) => {
	const statusParam = url.searchParams.get('status') ?? undefined;

	const parsed = csvExportSchema.safeParse({ status: statusParam });
	if (!parsed.success) {
		return new Response(JSON.stringify({ error: parsed.error.issues[0]?.message }), {
			status: 400,
			headers: { 'content-type': 'application/json' }
		});
	}

	const result = await requestCsvExport({
		userRole: locals.user?.role,
		status: parsed.data.status
	});

	if (!result.success) {
		return new Response(JSON.stringify({ error: result.reason }), {
			status: result.status,
			headers: { 'content-type': 'application/json' }
		});
	}

	return new Response(result.csv, {
		status: 200,
		headers: {
			'content-type': 'text/csv; charset=utf-8',
			'content-disposition': `attachment; filename="${result.filename}"`
		}
	});
};
