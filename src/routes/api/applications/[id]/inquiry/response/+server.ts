import { error, json } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { answerInquiryForApplication } from '$lib/server/services/application-inquiry.service';
import { applicationInquiryResponseSchema } from '$lib/server/services/validation';
import { ZodError } from 'zod';

export const POST: RequestHandler = async ({ params, request, locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	if (locals.user.role !== 'applicant') {
		throw error(403, 'Keine Berechtigung');
	}

	const applicationId = Number.parseInt(params.id, 10);

	if (!Number.isFinite(applicationId)) {
		throw error(400, 'Ungültige Antrags-ID');
	}

	try {
		const payload = applicationInquiryResponseSchema.parse(await request.json());
		const inquiry = await answerInquiryForApplication({
			applicationId,
			applicantEmail: locals.user.email,
			responseText: payload.responseText
		});

		return json(inquiry, { status: 200 });
	} catch (err) {
		if (err instanceof ZodError) {
			return json({ errors: err.flatten() }, { status: 400 });
		}
		throw err;
	}
};
