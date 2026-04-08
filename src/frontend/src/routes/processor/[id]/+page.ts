import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';
import type { Application, ApplicationInquiry } from '$lib/types';
import { handleApiResponse } from '$lib/api';

export const load: PageLoad = async ({ fetch, params, url }) => {
	const id = parseInt(params.id);

	if (isNaN(id)) {
		throw error(400, 'Ungültige Antrags-ID');
	}

	const res = await fetch(`/api/processor/${id}`);
	if (res.status === 404) {
		throw error(404, 'Antrag nicht gefunden');
	}
	const application = await handleApiResponse<Application>(res, url, 'Fehler beim Laden');

	let inquiries: ApplicationInquiry[] = [];
	try {
		const inquiriesRes = await fetch(`/api/applications/${id}/inquiries`);
		if (inquiriesRes.ok) {
			inquiries = await inquiriesRes.json();
		}
	} catch {
		// ignore fetch errors for inquiries
	}

	return { application, inquiries };
};
