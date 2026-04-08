import { test, expect } from './fixtures';

test.describe('Risikomanager (Risk Manager) Workflows', () => {
	test.describe('Scoring-Konfiguration', () => {
		test.use({ userRole: 'risk_manager' });

		test('should navigate to scoring config page', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/risk-manager/scoring-config');
			await expect(authenticatedPage.getByTestId('scoring-config-page')).toBeVisible();
		});

		test('should display scoring config form with fields', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/risk-manager/scoring-config');
			await expect(authenticatedPage.getByTestId('scoring-config-green-threshold')).toBeVisible();
			await expect(authenticatedPage.getByTestId('scoring-config-yellow-threshold')).toBeVisible();
			await expect(
				authenticatedPage.getByTestId('scoring-config-penalty-payment-default')
			).toBeVisible();
		});

		test('should show save button', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/risk-manager/scoring-config');
			await expect(authenticatedPage.getByTestId('scoring-config-save')).toBeVisible();
		});

		test('should show rescore button', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/risk-manager/scoring-config');
			await expect(authenticatedPage.getByTestId('scoring-config-rescore')).toBeVisible();
		});

		test('should show nav link for scoring config', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/');
			await expect(authenticatedPage.getByTestId('nav-scoring-config')).toBeVisible();
		});
	});
});
