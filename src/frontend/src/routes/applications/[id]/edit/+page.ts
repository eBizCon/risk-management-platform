import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';
import type { Application } from '$lib/types';
import { handleApiResponse } from '$lib/api';

export const load: PageLoad = async ({ fetch, params, url }) => {
	const id = parseInt(params.id);

	if (isNaN(id)) {
		throw error(400, 'Ungültige Antrags-ID');
	}

	const res = await fetch(`/api/applications/${id}`);
	if (res.status === 404) {
		throw error(404, 'Antrag nicht gefunden');
	}
	const application = await handleApiResponse<Application>(res, url, 'Fehler beim Laden');

	if (application.status !== 'draft') {
		throw error(403, 'Nur Entwürfe können bearbeitet werden');
	}

	return { application };
};
