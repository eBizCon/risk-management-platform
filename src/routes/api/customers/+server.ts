import { json, error } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { fetchCustomers } from '$lib/server/services/jhipster-client';

export const GET: RequestHandler = async ({ locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	try {
		const customers = await fetchCustomers();
		return json(customers);
	} catch (err) {
		console.error('Failed to fetch customers from JHipster:', err);
		return json([], { status: 200 });
	}
};
