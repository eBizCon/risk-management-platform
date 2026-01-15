import { json, error } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { submitApplication } from '$lib/server/services/repository';

export const POST: RequestHandler = async ({ params }) => {
	const id = parseInt(params.id);
	
	if (isNaN(id)) {
		throw error(400, 'Ungültige Antrags-ID');
	}

	const application = await submitApplication(id);
	
	if (!application) {
		throw error(400, 'Antrag konnte nicht eingereicht werden (nur Entwürfe können eingereicht werden)');
	}

	return json(application);
};
