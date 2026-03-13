import type { LayoutLoad } from './$types';

export const ssr = false;

export const load: LayoutLoad = async ({ fetch }) => {
	try {
		const res = await fetch('/api/auth/user');
		if (res.ok) {
			const data = await res.json();
			if (data && data.id) {
				return { user: data };
			}
		}
	} catch {
		// ignore fetch errors
	}
	return { user: null };
};
