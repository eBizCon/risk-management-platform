import { defineConfig } from 'vitest/config';
import { svelte } from '@sveltejs/vite-plugin-svelte';
import { svelteTesting } from '@testing-library/svelte/vite';

export default defineConfig({
	plugins: [svelte({ hot: !process.env.VITEST }), svelteTesting()],
	test: {
		include: ['src/**/*.{test,spec}.{js,ts}'],
		globals: true,
		environment: 'jsdom',
		setupFiles: ['./vitest.setup.ts'],
		passWithNoTests: true
	},
	resolve: {
		alias: {
			$lib: '/src/lib',
			'$app/environment': '/src/lib/test/app-environment'
		}
	}
});
