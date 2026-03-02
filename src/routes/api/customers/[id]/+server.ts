import { json, error } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { fetchCustomerById } from '$lib/server/services/customer.client';

export const GET: RequestHandler = async ({ params, locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	const id = parseInt(params.id);

	if (isNaN(id)) {
		throw error(400, 'Ungültige Kunden-ID');
	}

	const customer = await fetchCustomerById(id);

	if (!customer) {
		throw error(404, 'Kunde nicht gefunden');
	}

	return json(customer);
};
