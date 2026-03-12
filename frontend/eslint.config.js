import js from '@eslint/js';
import { defineConfig } from 'eslint/config';
import tseslint from 'typescript-eslint';
import svelte from 'eslint-plugin-svelte';
import prettier from 'eslint-config-prettier';
import globals from 'globals';

export default defineConfig(
	js.configs.recommended,
	tseslint.configs.recommended,
	...svelte.configs['flat/recommended'],
	prettier,
	...svelte.configs['flat/prettier'],
	{
		languageOptions: {
			globals: {
				...globals.browser,
				...globals.node
			}
		}
	},
	{
		files: ['**/*.svelte'],
		languageOptions: {
			parserOptions: {
				parser: tseslint.parser
			}
		}
	},
	{
		rules: {
			'@typescript-eslint/no-unused-vars': 'warn',
			'@typescript-eslint/no-explicit-any': 'warn',
			'no-console': 'warn',
			'no-unused-vars': 'off',
			'no-undef': 'off',
			'svelte/valid-compile': 'warn'
		}
	},
	{
		ignores: [
			'node_modules/',
			'.svelte-kit/',
			'build/',
			'dist/',
			'playwright-report/',
			'test-results/'
		]
	}
);
