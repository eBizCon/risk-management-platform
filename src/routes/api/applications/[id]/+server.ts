import { json, error } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import {
	deleteApplication,
	getApplicationById
} from '$lib/server/services/repositories/application.repository';

const ensureApplicantOwnership = (user: App.User | undefined, createdBy: string) => {
	if (!user) {
		throw error(401, 'Login erforderlich');
	}

	if (user.role !== 'applicant') {
		throw error(403, 'Keine Berechtigung');
	}

	if (createdBy !== user.email) {
		throw error(403, 'Keine Berechtigung');
	}
};

export const DELETE: RequestHandler = async ({ params, locals }) => {
	const id = parseInt(params.id);

	if (isNaN(id)) {
		throw error(400, 'Ungültige Antrags-ID');
	}

	const existing = await getApplicationById(id);

	if (!existing) {
		throw error(404, 'Antrag nicht gefunden');
	}

	ensureApplicantOwnership(locals.user, existing.createdBy);

	const success = await deleteApplication(id);

	if (!success) {
		throw error(400, 'Antrag konnte nicht gelöscht werden (nur Entwürfe können gelöscht werden)');
	}

	return json({ success: true });
};

export const GET: RequestHandler = async ({ params, locals }) => {
	const id = parseInt(params.id);

	if (isNaN(id)) {
		throw error(400, 'Ungültige Antrags-ID');
	}

	const application = await getApplicationById(id);

	if (!application) {
		throw error(404, 'Antrag nicht gefunden');
	}

	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	if (locals.user.role === 'applicant' && application.createdBy !== locals.user.email) {
		throw error(403, 'Keine Berechtigung');
	}

	if (locals.user.role !== 'applicant' && locals.user.role !== 'processor') {
		throw error(403, 'Keine Berechtigung');
	}

	return json(application);
};
