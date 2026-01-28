import type { PageServerLoad, Actions } from './$types';
import { error, fail, redirect } from '@sveltejs/kit';
import { getApplicationById, processApplication } from '$lib/server/services/repositories/application.repository';
import { processorDecisionSchema } from '$lib/server/services/validation';
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

	return {
		application
	};
};

export const actions: Actions = {
	default: async ({ request, params }) => {
		const id = parseInt(params.id);
		const formData = await request.formData();
		
		const rawData = {
			decision: formData.get('decision') as string,
			comment: formData.get('comment') as string || undefined
		};

		try {
			const validatedData = processorDecisionSchema.parse(rawData);

			const application = await processApplication(
				id,
				validatedData.decision as 'approved' | 'rejected',
				validatedData.comment
			);

			if (!application) {
				throw error(400, 'Antrag kann nicht bearbeitet werden (möglicherweise nicht im Status "Eingereicht")');
			}

			throw redirect(303, `/processor/${id}?processed=true`);
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
