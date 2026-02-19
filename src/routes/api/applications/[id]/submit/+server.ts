import { json, error } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import {
	submitApplication,
	getApplicationById
} from '$lib/server/services/repositories/application.repository';

export const POST: RequestHandler = async ({ params, locals }) => {
	const id = parseInt(params.id);

	if (isNaN(id)) {
		throw error(400, 'Ungültige Antrags-ID');
	}

	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	if (locals.user.role !== 'applicant') {
		throw error(403, 'Keine Berechtigung');
	}

	const existing = await getApplicationById(id);

	if (!existing) {
		throw error(404, 'Antrag nicht gefunden');
	}

	if (existing.createdBy !== locals.user.email) {
		throw error(403, 'Keine Berechtigung');
	}

	const application = await submitApplication(id);

	if (!application) {
		throw error(
			400,
			'Antrag konnte nicht eingereicht werden (nur Entwürfe können eingereicht werden)'
		);
	}

	return json(application);
};
