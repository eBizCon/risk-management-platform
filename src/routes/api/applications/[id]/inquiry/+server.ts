import { error, json } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { createInquiryForApplication } from '$lib/server/services/application-inquiry.service';
import { applicationInquirySchema } from '$lib/server/services/validation';
import { ZodError } from 'zod';

export const POST: RequestHandler = async ({ params, request, locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	if (locals.user.role !== 'processor') {
		throw error(403, 'Keine Berechtigung');
	}

	const applicationId = Number.parseInt(params.id, 10);

	if (!Number.isFinite(applicationId)) {
		throw error(400, 'Ungültige Antrags-ID');
	}

	try {
		const payload = applicationInquirySchema.parse(await request.json());
		const inquiry = await createInquiryForApplication({
			applicationId,
			processorEmail: locals.user.email,
			inquiryText: payload.inquiryText
		});

		return json(inquiry, { status: 201 });
	} catch (err) {
		if (err instanceof ZodError) {
			return json({ errors: err.flatten() }, { status: 400 });
		}
		throw err;
	}
};
