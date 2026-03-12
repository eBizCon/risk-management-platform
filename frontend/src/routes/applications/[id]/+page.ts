import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch, params }) => {
	const id = parseInt(params.id);

	if (isNaN(id)) {
		throw error(400, 'Ungültige Antrags-ID');
	}

	const res = await fetch(`/api/applications/${id}`);
	if (!res.ok) {
		if (res.status === 404) {
			throw error(404, 'Antrag nicht gefunden');
		}
		throw error(res.status, 'Fehler beim Laden');
	}

	const application = await res.json();

	let inquiries: unknown[] = [];
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
