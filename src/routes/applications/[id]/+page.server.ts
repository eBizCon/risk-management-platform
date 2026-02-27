import type { PageServerLoad } from './$types';
import { error } from '@sveltejs/kit';
import { getApplicationById } from '$lib/server/services/repositories/application.repository';
import { fetchCustomerById } from '$lib/server/services/jhipster-client';

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

	let customer = null;
	if (application.customerId) {
		try {
			customer = await fetchCustomerById(application.customerId);
		} catch (err) {
			console.error('Failed to fetch customer from JHipster:', err);
		}
	}

	return {
		application,
		customer
	};
};
