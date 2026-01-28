import { defineConfig } from 'vitest/config';
import { svelte } from '@sveltejs/vite-plugin-svelte';

export default defineConfig({
	plugins: [svelte({ hot: !process.env.VITEST })],
	test: {
		include: ['src/**/*.{test,spec}.{js,ts}'],
		globals: true,
		environment: 'jsdom',
		setupFiles: ['./vitest.setup.ts']
	},
	resolve: {
		alias: {
			$lib: '/src/lib',
			'$app/environment': '/src/lib/test/app-environment',
			'$env/static/private': '/src/lib/test/env-static-private',
			'$env/dynamic/private': '/src/lib/test/env-dynamic-private'
		}
	}
});
