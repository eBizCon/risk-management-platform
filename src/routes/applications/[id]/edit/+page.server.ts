import type { PageServerLoad, Actions } from './$types';
import { error, fail, redirect } from '@sveltejs/kit';
import { getApplicationById, updateApplication, submitApplication } from '$lib/server/services/repositories/application.repository';
import { applicationWithBusinessRulesSchema } from '$lib/server/services/validation';
import { ZodError } from 'zod';

export const load: PageServerLoad = async ({ params }) => {
	const id = parseInt(params.id);
	
	if (isNaN(id)) {
		throw error(400, 'Ungültige Antrags-ID');
	}

	const application = await getApplicationById(id);
	
	if (!application) {
		throw error(404, 'Antrag nicht gefunden');
	}

	if (application.status !== 'draft') {
		throw error(403, 'Nur Entwürfe können bearbeitet werden');
	}

	return {
		application
	};
};

export const actions: Actions = {
	default: async ({ request, params }) => {
		const id = parseInt(params.id);
		const formData = await request.formData();
		
		const rawData = {
			name: formData.get('name') as string,
			income: parseFloat(formData.get('income') as string),
			fixedCosts: parseFloat(formData.get('fixedCosts') as string),
			desiredRate: parseFloat(formData.get('desiredRate') as string),
			employmentStatus: formData.get('employmentStatus') as string,
			hasPaymentDefault: formData.get('hasPaymentDefault') === 'true'
		};

		try {
			const validatedData = applicationWithBusinessRulesSchema.parse(rawData);
			const action = formData.get('action');

			const application = await updateApplication(id, validatedData);

			if (!application) {
				throw error(404, 'Antrag nicht gefunden oder kann nicht bearbeitet werden');
			}

			if (action === 'submit') {
				await submitApplication(id);
				throw redirect(303, `/applications/${id}?submitted=true`);
			}

			throw redirect(303, `/applications/${id}`);
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
