import { error, redirect } from '@sveltejs/kit';

export async function handleApiResponse<T>(
	response: Response,
	url: URL,
	fallbackMessage = 'Fehler beim Laden'
): Promise<T> {
	if (!response.ok) {
		if (response.status === 401) {
			const returnTo = encodeURIComponent(url.pathname + url.search);
			redirect(302, `/login?returnTo=${returnTo}`);
		}
		throw error(response.status, fallbackMessage);
	}
	return response.json();
}
