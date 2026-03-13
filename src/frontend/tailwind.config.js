import forms from '@tailwindcss/forms';
import typography from '@tailwindcss/typography';

/** @type {import('tailwindcss').Config} */
export default {
	content: ['./src/**/*.{html,js,svelte,ts}'],
	theme: {
		screens: {
			sm: '768px',
			md: '1024px',
			lg: '1280px'
		},
		extend: {}
	},
	plugins: [forms, typography]
};
