import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';

const backendUrl = 'http://localhost:5000';

export default defineConfig({
	plugins: [sveltekit()],
	server: {
		proxy: {
			'/api': backendUrl,
			'/login': backendUrl,
			'/logout': backendUrl,
			'/auth': backendUrl
		}
	},
	preview: {
		proxy: {
			'/api': backendUrl,
			'/login': backendUrl,
			'/logout': backendUrl,
			'/auth': backendUrl
		}
	}
});
