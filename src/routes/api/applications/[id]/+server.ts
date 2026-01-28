import { json, error } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { deleteApplication, getApplicationById } from '$lib/server/services/repositories/application.repository';

export const DELETE: RequestHandler = async ({ params }) => {
	const id = parseInt(params.id);
	
	if (isNaN(id)) {
		throw error(400, 'Ungültige Antrags-ID');
	}

	const success = await deleteApplication(id);
	
	if (!success) {
		throw error(400, 'Antrag konnte nicht gelöscht werden (nur Entwürfe können gelöscht werden)');
	}

	return json({ success: true });
};

export const GET: RequestHandler = async ({ params }) => {
	const id = parseInt(params.id);
	
	if (isNaN(id)) {
		throw error(400, 'Ungültige Antrags-ID');
	}

	const application = await getApplicationById(id);
	
	if (!application) {
		throw error(404, 'Antrag nicht gefunden');
	}

	return json(application);
};
