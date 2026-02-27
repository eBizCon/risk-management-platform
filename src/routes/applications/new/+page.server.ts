import type { PageServerLoad, Actions } from './$types';
import { fail, redirect, error } from '@sveltejs/kit';
import {
	createApplication,
	submitApplication
} from '$lib/server/services/repositories/application.repository';
import { applicationWithBusinessRulesSchema } from '$lib/server/services/validation';
import { fetchCustomers } from '$lib/server/services/jhipster-client';
import { ZodError } from 'zod';

export const load: PageServerLoad = async ({ locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	let customers: Awaited<ReturnType<typeof fetchCustomers>> = [];
	try {
		customers = await fetchCustomers();
	} catch (err) {
		console.error('Failed to fetch customers from JHipster:', err);
	}

	return { customers };
};

export const actions: Actions = {
	default: async ({ request, locals }) => {
		if (!locals.user) {
			throw error(401, 'Login erforderlich');
		}

		const formData = await request.formData();
		const customerIdRaw = formData.get('customerId') as string;
		const rawData = {
			name: formData.get('name') as string,
			income: parseFloat(formData.get('income') as string),
			fixedCosts: parseFloat(formData.get('fixedCosts') as string),
			desiredRate: parseFloat(formData.get('desiredRate') as string),
			employmentStatus: formData.get('employmentStatus') as string,
			hasPaymentDefault: formData.get('hasPaymentDefault') === 'true',
			customerId: customerIdRaw ? parseInt(customerIdRaw, 10) : null
		};

		try {
			const validatedData = applicationWithBusinessRulesSchema.parse(rawData);
			const action = formData.get('action');

			const application = await createApplication({
				...validatedData,
				status: 'draft',
				createdBy: locals.user.email
			});

			if (action === 'submit' && application) {
				await submitApplication(application.id);
				throw redirect(303, `/applications/${application.id}?submitted=true`);
			}

			throw redirect(303, `/applications/${application.id}`);
		} catch (error) {
			if (error instanceof ZodError) {
				const errors: Record<string, string[]> = {};
				error.issues.forEach((issue) => {
					const path = issue.path.join('.');
					if (!errors[path]) {
						errors[path] = [];
					}
					errors[path].push(issue.message);
				});
				return fail(400, { errors, values: rawData });
			}
			throw error;
		}
	}
};
