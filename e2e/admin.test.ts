import { test, expect } from './fixtures';
import { createTestSession, clearTestSessions } from './helpers/auth';

test.describe('Admin - Scoring-Konfiguration einsehen', () => {
	test.use({ userRole: 'admin' });

	test('should display current scoring configuration', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/admin/scoring-config');
		await expect(authenticatedPage.getByTestId('admin-scoring-form')).toBeVisible();
		await expect(authenticatedPage.getByTestId('input-traffic-light-green')).toBeVisible();
		await expect(authenticatedPage.getByTestId('input-traffic-light-yellow')).toBeVisible();
		await expect(authenticatedPage.getByTestId('input-income-excellent')).toBeVisible();
		await expect(authenticatedPage.getByTestId('input-income-good')).toBeVisible();
		await expect(authenticatedPage.getByTestId('input-income-moderate')).toBeVisible();
		await expect(authenticatedPage.getByTestId('input-afford-comfortable')).toBeVisible();
		await expect(authenticatedPage.getByTestId('input-afford-moderate')).toBeVisible();
		await expect(authenticatedPage.getByTestId('input-afford-stretched')).toBeVisible();
		await expect(authenticatedPage.getByTestId('input-emp-employed')).toBeVisible();
		await expect(authenticatedPage.getByTestId('input-emp-self-employed')).toBeVisible();
		await expect(authenticatedPage.getByTestId('input-emp-retired')).toBeVisible();
		await expect(authenticatedPage.getByTestId('input-emp-unemployed')).toBeVisible();
		await expect(authenticatedPage.getByTestId('input-payment-default-deduction')).toBeVisible();
	});

	test('should display default values in form fields', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/admin/scoring-config');
		await expect(authenticatedPage.getByTestId('input-traffic-light-green')).toHaveValue('75');
		await expect(authenticatedPage.getByTestId('input-traffic-light-yellow')).toHaveValue('50');
	});

	test('should show admin navigation link', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await expect(authenticatedPage.getByTestId('nav-admin-scoring')).toBeVisible();
	});
});

test.describe('Admin - Scoring-Konfiguration ändern', () => {
	test.use({ userRole: 'admin' });

	test('should update scoring configuration', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/admin/scoring-config');
		await authenticatedPage.getByTestId('input-traffic-light-green').fill('80');
		await authenticatedPage.getByTestId('btn-save-scoring-config').click();
		await expect(authenticatedPage.getByTestId('scoring-config-success')).toBeVisible();
		await expect(authenticatedPage.getByText(/erfolgreich gespeichert/i)).toBeVisible();

		await authenticatedPage.reload();
		await expect(authenticatedPage.getByTestId('input-traffic-light-green')).toHaveValue('80');

		await authenticatedPage.getByTestId('input-traffic-light-green').fill('75');
		await authenticatedPage.getByTestId('btn-save-scoring-config').click();
		await expect(authenticatedPage.getByTestId('scoring-config-success')).toBeVisible();
	});
});

test.describe('Admin - Ungültige Werte ablehnen', () => {
	test.use({ userRole: 'admin' });

	test('should reject invalid threshold values where yellow >= green', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/admin/scoring-config');
		await authenticatedPage.getByTestId('input-traffic-light-green').fill('50');
		await authenticatedPage.getByTestId('input-traffic-light-yellow').fill('60');
		await authenticatedPage.getByTestId('btn-save-scoring-config').click();
		await expect(authenticatedPage.getByTestId('scoring-config-error')).toContainText(/Gelb-Schwelle/i);
	});

	test('should reject when yellow equals green', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/admin/scoring-config');
		await authenticatedPage.getByTestId('input-traffic-light-green').fill('50');
		await authenticatedPage.getByTestId('input-traffic-light-yellow').fill('50');
		await authenticatedPage.getByTestId('btn-save-scoring-config').click();
		await expect(authenticatedPage.getByTestId('scoring-config-error')).toBeVisible();
	});
});

test.describe('Admin - Zugriffskontrolle', () => {
	test('should deny access for applicant users', async ({ browser }) => {
		const context = await browser.newContext();
		const page = await context.newPage();
		await createTestSession(page, 'applicant');

		const response = await page.request.get('/api/admin/scoring-config');
		expect(response.status()).toBe(403);

		await clearTestSessions(page);
		await context.close();
	});

	test('should deny access for processor users', async ({ browser }) => {
		const context = await browser.newContext();
		const page = await context.newPage();
		await createTestSession(page, 'processor');

		const response = await page.request.get('/api/admin/scoring-config');
		expect(response.status()).toBe(403);

		await clearTestSessions(page);
		await context.close();
	});
});

test.describe('Admin - Bestehende Anträge unverändert', () => {
	test('should not change existing application score when config changes', async ({ browser }) => {
		const applicantContext = await browser.newContext();
		const applicantPage = await applicantContext.newPage();
		await createTestSession(applicantPage, 'applicant');

		await applicantPage.goto('/applications/new');
		await applicantPage.getByTestId('input-name').fill('Config Test User');
		await applicantPage.getByTestId('input-income').fill('5000');
		await applicantPage.getByTestId('input-fixed-costs').fill('2000');
		await applicantPage.getByTestId('input-desired-rate').fill('500');
		await applicantPage.getByTestId('select-employment-status').selectOption('employed');
		await applicantPage.getByTestId('radio-payment-default-no').check();
		await applicantPage.getByTestId('btn-submit-application').click();
		await applicantPage.getByTestId('confirm-dialog-confirm').click();

		await expect(applicantPage).toHaveURL(/\/applications\/\d+/, { timeout: 10000 });
		const originalScoreText = await applicantPage.getByTestId('score-value').textContent();

		const applicationUrl = applicantPage.url();

		await clearTestSessions(applicantPage);
		await applicantContext.close();

		const adminContext = await browser.newContext();
		const adminPage = await adminContext.newPage();
		await createTestSession(adminPage, 'admin');

		await adminPage.goto('/admin/scoring-config');
		await adminPage.getByTestId('input-traffic-light-green').fill('95');
		await adminPage.getByTestId('input-payment-default-deduction').fill('50');
		await adminPage.getByTestId('btn-save-scoring-config').click();
		await expect(adminPage.getByTestId('scoring-config-success')).toBeVisible();

		await clearTestSessions(adminPage);
		await adminContext.close();

		const verifyContext = await browser.newContext();
		const verifyPage = await verifyContext.newPage();
		await createTestSession(verifyPage, 'applicant');

		await verifyPage.goto(applicationUrl);
		const newScoreText = await verifyPage.getByTestId('score-value').textContent();

		expect(newScoreText).toBe(originalScoreText);

		await adminPage.goto('/admin/scoring-config');
		const restoreAdminContext = await browser.newContext();
		const restoreAdminPage = await restoreAdminContext.newPage();
		await createTestSession(restoreAdminPage, 'admin');
		await restoreAdminPage.goto('/admin/scoring-config');
		await restoreAdminPage.getByTestId('input-traffic-light-green').fill('75');
		await restoreAdminPage.getByTestId('input-payment-default-deduction').fill('25');
		await restoreAdminPage.getByTestId('btn-save-scoring-config').click();

		await clearTestSessions(verifyPage);
		await verifyContext.close();
		await clearTestSessions(restoreAdminPage);
		await restoreAdminContext.close();
	});
});
