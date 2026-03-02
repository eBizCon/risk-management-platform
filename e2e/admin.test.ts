import { test, expect } from './fixtures';
import { createTestSession, clearTestSessions } from './helpers/auth';

test.describe('Admin Scoring Configuration', () => {
	test.describe('Zugriffskontrolle (Access Control)', () => {
		test('should deny access to /admin/scoring for applicant role', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'applicant');

			const response = await page.goto('/admin/scoring');
			expect(response?.status()).toBe(403);
		});

		test('should deny access to /admin/scoring for processor role', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'processor');

			const response = await page.goto('/admin/scoring');
			expect(response?.status()).toBe(403);
		});

		test('should allow access to /admin/scoring for admin role', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'admin');

			await page.goto('/admin/scoring');
			await expect(page.getByTestId('admin-scoring-heading')).toBeVisible();
		});

		test('should deny API access to scoring config for applicant', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'applicant');

			const response = await page.request.get('/api/admin/scoring-config');
			expect(response.status()).toBe(403);
		});

		test('should deny API access to scoring config for processor', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'processor');

			const response = await page.request.get('/api/admin/scoring-config');
			expect(response.status()).toBe(403);
		});
	});

	test.describe('Konfiguration anzeigen und ändern (View and Change Config)', () => {
		test('should display current scoring configuration', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'admin');

			await page.goto('/admin/scoring');

			await expect(page.getByTestId('input-green-threshold')).toBeVisible();
			await expect(page.getByTestId('input-yellow-threshold')).toBeVisible();
			await expect(page.getByTestId('input-weight-income')).toBeVisible();
			await expect(page.getByTestId('input-weight-fixed-costs')).toBeVisible();
			await expect(page.getByTestId('input-weight-employment')).toBeVisible();
			await expect(page.getByTestId('input-weight-payment-default')).toBeVisible();
		});

		test('should save new scoring configuration', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'admin');

			await page.goto('/admin/scoring');

			await page.getByTestId('input-green-threshold').fill('80');
			await page.getByTestId('input-yellow-threshold').fill('55');

			await page.getByTestId('btn-save-config').click();

			await expect(page.getByTestId('success-message')).toBeVisible();
		});

		test('should show validation error when yellow >= green threshold', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'admin');

			await page.goto('/admin/scoring');

			await page.getByTestId('input-green-threshold').fill('50');
			await page.getByTestId('input-yellow-threshold').fill('60');

			await page.getByTestId('btn-save-config').click();

			await expect(page.getByTestId('error-message')).toBeVisible();
		});

		test('should reset to default values', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'admin');

			await page.goto('/admin/scoring');

			await page.getByTestId('input-green-threshold').fill('90');
			await page.getByTestId('btn-reset-config').click();

			await expect(page.getByTestId('input-green-threshold')).toHaveValue('75');
			await expect(page.getByTestId('input-yellow-threshold')).toHaveValue('50');
		});
	});

	test.describe('Auswirkung auf neue Anträge (Effect on New Applications)', () => {
		test('should apply new config to new applications', async ({ page }) => {
			// Step 1: As admin, change green threshold to 80
			await clearTestSessions(page);
			await createTestSession(page, 'admin');

			await page.goto('/admin/scoring');
			await page.getByTestId('input-green-threshold').fill('80');
			await page.getByTestId('input-yellow-threshold').fill('50');
			await page.getByTestId('btn-save-config').click();
			await expect(page.getByTestId('success-message')).toBeVisible();

			// Step 2: As applicant, create and submit an application that would score ~75-79
			// With default config this would be green, but with greenThreshold=80 it should be yellow
			await clearTestSessions(page);
			await createTestSession(page, 'applicant');

			await page.goto('/applications/new');
			await page.getByTestId('input-name').fill('Config Test User');
			// income=5000, fixedCosts=2000, available=3000 (60% ratio -> no penalty)
			// desiredRate=600 (20% of available -> no penalty)
			// employed -> no penalty
			// no payment default -> no penalty
			// Base score = 100, minus 0 = 100... too high
			// Let's use values that produce a score around 75-79
			// income=4000, fixedCosts=2200, available=1800 (45% ratio -> -15)
			// rate=500, ratio=27.7% -> no penalty
			// self_employed -> -10
			// no default -> 0
			// Score = 100 - 15 - 10 = 75 -> with greenThreshold=80 should be yellow
			await page.getByTestId('input-income').fill('4000');
			await page.getByTestId('input-fixed-costs').fill('2200');
			await page.getByTestId('input-desired-rate').fill('500');
			await page.getByTestId('select-employment-status').selectOption('self_employed');
			await page.getByTestId('radio-payment-default-no').check();

			await page.getByTestId('btn-submit-application').click();
			await page.getByTestId('confirm-dialog-confirm').click();

			await expect(page).toHaveURL(/\/applications\/\d+/);
			// Score 75 with greenThreshold=80 should yield yellow
			await expect(page.getByTestId('traffic-light-yellow')).toBeVisible();
		});

		test('should not affect existing applications after config change', async ({ page }) => {
			// Step 1: As applicant, create and submit application with default config
			await clearTestSessions(page);
			await createTestSession(page, 'applicant');

			await page.goto('/applications/new');
			await page.getByTestId('input-name').fill('Existing App Test');
			await page.getByTestId('input-income').fill('6000');
			await page.getByTestId('input-fixed-costs').fill('2000');
			await page.getByTestId('input-desired-rate').fill('500');
			await page.getByTestId('select-employment-status').selectOption('employed');
			await page.getByTestId('radio-payment-default-no').check();

			await page.getByTestId('btn-submit-application').click();
			await page.getByTestId('confirm-dialog-confirm').click();

			await expect(page).toHaveURL(/\/applications\/\d+/);
			const url = page.url();

			// Verify it has green traffic light
			await expect(page.getByTestId('traffic-light-green')).toBeVisible();

			// Step 2: As admin, change thresholds drastically
			await clearTestSessions(page);
			await createTestSession(page, 'admin');

			await page.goto('/admin/scoring');
			await page.getByTestId('input-green-threshold').fill('99');
			await page.getByTestId('input-yellow-threshold').fill('90');
			await page.getByTestId('btn-save-config').click();
			await expect(page.getByTestId('success-message')).toBeVisible();

			// Step 3: Go back and verify the existing application still has green traffic light
			await clearTestSessions(page);
			await createTestSession(page, 'applicant');

			await page.goto(url);
			await expect(page.getByTestId('traffic-light-green')).toBeVisible();
		});
	});

	test.describe('Navigation', () => {
		test('should show admin nav link for admin users', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'admin');

			await page.goto('/');
			await expect(page.getByTestId('nav-admin-scoring')).toBeVisible();
		});

		test('should not show admin nav link for applicant users', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'applicant');

			await page.goto('/');
			await expect(page.getByTestId('nav-admin-scoring')).not.toBeVisible();
		});

		test('should not show admin nav link for processor users', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'processor');

			await page.goto('/');
			await expect(page.getByTestId('nav-admin-scoring')).not.toBeVisible();
		});
	});
});
