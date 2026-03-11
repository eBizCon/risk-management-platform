import type { Actions, PageServerLoad } from './$types';
import { error, fail, redirect } from '@sveltejs/kit';
import {
	answerInquiryForApplication,
	getApplicationInquiryTimeline
} from '$lib/server/services/application-inquiry.service';
import { getApplicationById } from '$lib/server/services/repositories/application.repository';
import { applicationInquiryResponseSchema } from '$lib/server/services/validation';
import { ZodError } from 'zod';

export const load: PageServerLoad = async ({ params, locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	const id = parseInt(params.id);

	if (isNaN(id)) {
		throw error(400, 'Ungültige Antrags-ID');
	}

	const application = await getApplicationById(id);

	if (!application) {
		throw error(404, 'Antrag nicht gefunden');
	}

	if (locals.user.role !== 'applicant') {
		throw error(403, 'Keine Berechtigung');
	}

	if (application.createdBy !== locals.user.email) {
		throw error(403, 'Keine Berechtigung');
	}

	return {
		application,
		inquiries: await getApplicationInquiryTimeline(id)
	};
};

export const actions: Actions = {
	answerInquiry: async ({ request, params, locals }) => {
		if (!locals.user) {
			throw error(401, 'Login erforderlich');
		}

		if (locals.user.role !== 'applicant') {
			throw error(403, 'Keine Berechtigung');
		}

		const id = Number.parseInt(params.id, 10);
		const formData = await request.formData();
		const rawData = {
			responseText: (formData.get('responseText') as string) ?? ''
		};

		try {
			const validatedData = applicationInquiryResponseSchema.parse(rawData);
			await answerInquiryForApplication({
				applicationId: id,
				applicantEmail: locals.user.email,
				responseText: validatedData.responseText
			});
			throw redirect(303, `/applications/${id}?answeredInquiry=true`);
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
				return fail(400, { answerInquiryErrors: errors, answerInquiryValues: rawData });
			}
			throw err;
		}
	}
};
