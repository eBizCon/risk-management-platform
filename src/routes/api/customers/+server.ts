import { json, error } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { fetchCustomers } from '$lib/server/services/customer.client';

export const GET: RequestHandler = async ({ locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	const customers = await fetchCustomers();
	return json(customers);
};
