import { test, expect } from './fixtures';

test.describe('Admin Scoring-Konfiguration', () => {
	test.use({ userRole: 'admin' });

	test('sollte Admin-UI anzeigen', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/admin');
		await expect(authenticatedPage.getByTestId('scoring-config-form')).toBeVisible();
	});

	test('sollte Standardwerte anzeigen', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/admin');
		await expect(authenticatedPage.getByTestId('input-green-threshold')).toHaveValue('75');
		await expect(authenticatedPage.getByTestId('input-yellow-threshold')).toHaveValue('50');
	});

	test('sollte Schwellenwerte speichern können', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/admin');
		await authenticatedPage.getByTestId('input-green-threshold').fill('80');
		await authenticatedPage.getByTestId('input-yellow-threshold').fill('60');
		await authenticatedPage.getByTestId('btn-save-config').click();
		await expect(authenticatedPage.getByTestId('success-message')).toBeVisible();
	});

	test('sollte Admin-Navigation anzeigen', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await expect(authenticatedPage.getByTestId('nav-admin')).toBeVisible();
	});
});

test.describe('Admin Zugriffskontrolle', () => {
	test.use({ userRole: 'applicant' });

	test('sollte für applicant nicht zugänglich sein', async ({ authenticatedPage }) => {
		const response = await authenticatedPage.goto('/admin');
		expect(response?.status()).toBe(403);
	});
});
