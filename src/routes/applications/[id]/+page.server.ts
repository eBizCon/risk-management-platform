import type { PageServerLoad } from './$types';
import { error } from '@sveltejs/kit';
import { getApplicationById } from '$lib/server/services/repository';

export const load: PageServerLoad = async ({ params }) => {
	const id = parseInt(params.id);
	
	if (isNaN(id)) {
		throw error(400, 'Ungültige Antrags-ID');
	}

	const application = await getApplicationById(id);
	
	if (!application) {
		throw error(404, 'Antrag nicht gefunden');
	}

	return {
		application
	};
};
