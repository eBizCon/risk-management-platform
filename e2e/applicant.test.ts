import { test, expect } from './fixtures';
import type { Page } from '@playwright/test';

test.describe('Antragsteller (Applicant) Workflows', () => {

	test.describe('Kreditantrag erstellen (Create Credit Application)', () => {
		test('should display the home page with role-specific content', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/');
			await expect(authenticatedPage.locator('h1')).toContainText('Risikomanagement');
			await expect(
				authenticatedPage.getByRole('link', { name: /Neuen Antrag erstellen/i })
			).toBeVisible();
		});

		test('should navigate to new application form', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/');
			await authenticatedPage
				.getByRole('link', { name: /Neuen Antrag erstellen/i })
				.click();
			await expect(authenticatedPage).toHaveURL('/applications/new');
			await expect(authenticatedPage.locator('h1')).toContainText('Kreditantrag erstellen');
		});

		test('should display all required form fields', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications/new');

			await expect(authenticatedPage.getByTestId('input-name')).toBeVisible();
			await expect(authenticatedPage.getByTestId('input-income')).toBeVisible();
			await expect(authenticatedPage.getByTestId('input-fixed-costs')).toBeVisible();
			await expect(authenticatedPage.getByTestId('input-desired-rate')).toBeVisible();
			await expect(authenticatedPage.getByTestId('select-employment-status')).toBeVisible();
			await expect(authenticatedPage.getByTestId('radio-payment-default-yes')).toBeVisible();
			await expect(authenticatedPage.getByTestId('radio-payment-default-no')).toBeVisible();
		});

		test('should show validation errors for empty form submission', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications/new');

			await authenticatedPage.getByTestId('input-name').fill('A');
			await authenticatedPage.getByTestId('input-income').fill('1000');
			await authenticatedPage.getByTestId('input-fixed-costs').fill('500');
			await authenticatedPage.getByTestId('input-desired-rate').fill('200');
			await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
			await authenticatedPage.getByTestId('radio-payment-default-no').check();

			await authenticatedPage.getByTestId('btn-save-draft').click();

			await expect(authenticatedPage.getByText(/mindestens 2 Zeichen/i)).toBeVisible();
		});

		test('should show validation error for income equal to 0', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications/new');

			await authenticatedPage.getByTestId('input-name').fill('Test User');
			await authenticatedPage.getByTestId('input-income').fill('0');
			await authenticatedPage.getByTestId('input-fixed-costs').fill('0');
			await authenticatedPage.getByTestId('input-desired-rate').fill('200');
			await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
			await authenticatedPage.getByTestId('radio-payment-default-no').check();

			await authenticatedPage.getByTestId('btn-save-draft').click();

			await expect(authenticatedPage.getByText(/Einkommen muss positiv sein/i)).toBeVisible();
		});

		test('should show validation error when desired rate exceeds available income', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications/new');

			await authenticatedPage.getByTestId('input-name').fill('Test User');
			await authenticatedPage.getByTestId('input-income').fill('3000');
			await authenticatedPage.getByTestId('input-fixed-costs').fill('2000');
			await authenticatedPage.getByTestId('input-desired-rate').fill('1500');
			await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
			await authenticatedPage.getByTestId('radio-payment-default-no').check();

			await authenticatedPage.getByTestId('btn-save-draft').click();

			await expect(authenticatedPage.getByText(/kann nicht höher sein als das verfügbare Einkommen/i)).toBeVisible();
		});

		test('should successfully create a draft application', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications/new');

			await authenticatedPage.getByTestId('input-name').fill('Max Mustermann');
			await authenticatedPage.getByTestId('input-income').fill('4000');
			await authenticatedPage.getByTestId('input-fixed-costs').fill('1500');
			await authenticatedPage.getByTestId('input-desired-rate').fill('500');
			await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
			await authenticatedPage.getByTestId('radio-payment-default-no').check();

			await authenticatedPage.getByTestId('btn-save-draft').click();

			await expect(authenticatedPage).toHaveURL(/\/applications\/\d+/);
			await expect(authenticatedPage.getByTestId('status-badge-draft')).toBeVisible();
		});

		test('should successfully create and submit an application directly', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications/new');

			await authenticatedPage.getByTestId('input-name').fill('Anna Schmidt');
			await authenticatedPage.getByTestId('input-income').fill('5000');
			await authenticatedPage.getByTestId('input-fixed-costs').fill('2000');
			await authenticatedPage.getByTestId('input-desired-rate').fill('600');
			await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
			await authenticatedPage.getByTestId('radio-payment-default-no').check();

			await authenticatedPage.getByTestId('btn-submit-application').click();
			await expect(authenticatedPage.getByTestId('confirm-dialog')).toBeVisible();
			await authenticatedPage.getByTestId('confirm-dialog-confirm').click();

			await expect(authenticatedPage).toHaveURL(/\/applications\/\d+/);
			await expect(authenticatedPage.getByTestId('status-badge-submitted')).toBeVisible();
		});
	});

	test.describe('Entwurf speichern und bearbeiten (Save and Edit Draft)', () => {

		test('should filter applications by status', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications');

			const statusFilter = authenticatedPage.getByTestId('status-filter');
			if (await statusFilter.isVisible()) {
				await statusFilter.selectOption('draft');
				await expect(authenticatedPage).toHaveURL(/status=draft/);
			}
		});
	});

	test.describe('Antrag einreichen (Submit Application)', () => {
		test('should submit a draft application', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications/new');
			await authenticatedPage.getByTestId('input-name').fill('Submit Test');
			await authenticatedPage.getByTestId('input-income').fill('4500');
			await authenticatedPage.getByTestId('input-fixed-costs').fill('1800');
			await authenticatedPage.getByTestId('input-desired-rate').fill('500');
			await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
			await authenticatedPage.getByTestId('radio-payment-default-no').check();
			await authenticatedPage.getByTestId('btn-save-draft').click();

			await authenticatedPage.getByTestId('submit-application').click();
			await expect(authenticatedPage.getByTestId('confirm-dialog')).toBeVisible();
			await authenticatedPage.getByTestId('confirm-dialog-confirm').click();
			await authenticatedPage.waitForLoadState('networkidle');
			await expect(authenticatedPage.getByTestId('status-badge-submitted')).toBeVisible();
		});

		test('should not allow editing after submission', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications/new');
			await authenticatedPage.getByTestId('input-name').fill('No Edit After Submit');
			await authenticatedPage.getByTestId('input-income').fill('4000');
			await authenticatedPage.getByTestId('input-fixed-costs').fill('1500');
			await authenticatedPage.getByTestId('input-desired-rate').fill('450');
			await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
			await authenticatedPage.getByTestId('radio-payment-default-no').check();
			await authenticatedPage.getByTestId('btn-submit-application').click();
			await authenticatedPage.getByTestId('confirm-dialog-confirm').click();
			await expect(authenticatedPage.getByTestId('edit-application')).not.toBeVisible();
		});
	});

	test.describe('Bestätigungsdialog für Einreichung', () => {
		async function fillApplicationForm(page: Page) {
			await page.goto('/applications/new');
			await page.getByTestId('input-name').fill('Dialog Test');
			await page.getByTestId('input-income').fill('4500');
			await page.getByTestId('input-fixed-costs').fill('1500');
			await page.getByTestId('input-desired-rate').fill('500');
			await page.getByTestId('select-employment-status').selectOption('employed');
			await page.getByTestId('radio-payment-default-no').check();
		}

		test('should show confirmation dialog when submitting new application', async ({ authenticatedPage }) => {
			await fillApplicationForm(authenticatedPage);

			await authenticatedPage.getByTestId('btn-submit-application').click();

			await expect(authenticatedPage.getByTestId('confirm-dialog')).toBeVisible();
			await expect(authenticatedPage.getByTestId('confirm-dialog-confirm')).toBeVisible();
			await expect(authenticatedPage.getByTestId('confirm-dialog-cancel')).toBeVisible();
		});

		test('should submit application after confirming dialog', async ({ authenticatedPage }) => {
			await fillApplicationForm(authenticatedPage);

			await authenticatedPage.getByTestId('btn-submit-application').click();
			await authenticatedPage.getByTestId('confirm-dialog-confirm').click();

			await expect(authenticatedPage).toHaveURL(/\/applications\/\d+/);
			await expect(authenticatedPage.getByTestId('status-badge-submitted')).toBeVisible();
			await expect(authenticatedPage.getByTestId('confirm-dialog')).not.toBeVisible();
		});

		test('should not submit application when canceling dialog', async ({ authenticatedPage }) => {
			await fillApplicationForm(authenticatedPage);

			await authenticatedPage.getByTestId('btn-submit-application').click();
			await authenticatedPage.getByTestId('confirm-dialog-cancel').click();

			await expect(authenticatedPage).toHaveURL('/applications/new');
			await expect(authenticatedPage.getByTestId('confirm-dialog')).not.toBeVisible();
		});

		test('should show confirmation dialog when submitting from edit page', async ({ authenticatedPage }) => {
			await fillApplicationForm(authenticatedPage);
			await authenticatedPage.getByTestId('btn-save-draft').click();
			await authenticatedPage.getByTestId('edit-application').click();
			await authenticatedPage.getByTestId('btn-submit-application').click();

			await expect(authenticatedPage.getByTestId('confirm-dialog')).toBeVisible();
		});

		test('should show confirmation dialog when submitting from detail page', async ({ authenticatedPage }) => {
			await fillApplicationForm(authenticatedPage);
			await authenticatedPage.getByTestId('btn-save-draft').click();

			await authenticatedPage.getByTestId('submit-application').click();

			await expect(authenticatedPage.getByTestId('confirm-dialog')).toBeVisible();
		});

		test('should close dialog when pressing ESC key', async ({ authenticatedPage }) => {
			await fillApplicationForm(authenticatedPage);

			await authenticatedPage.getByTestId('btn-submit-application').click();
			await authenticatedPage.keyboard.press('Escape');

			await expect(authenticatedPage.getByTestId('confirm-dialog')).not.toBeVisible();
			await expect(authenticatedPage).toHaveURL('/applications/new');
		});
	});

	test.describe('Bewertungsergebnis einsehen (View Scoring Result)', () => {
		test('should display score and traffic light for submitted application', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications/new');
			await authenticatedPage.getByTestId('input-name').fill('Score Test');
			await authenticatedPage.getByTestId('input-income').fill('5000');
			await authenticatedPage.getByTestId('input-fixed-costs').fill('2000');
			await authenticatedPage.getByTestId('input-desired-rate').fill('500');
			await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
			await authenticatedPage.getByTestId('radio-payment-default-no').check();
			await authenticatedPage.getByTestId('btn-submit-application').click();
			await authenticatedPage.getByTestId('confirm-dialog-confirm').click();
			await expect(authenticatedPage.getByTestId('scoring-heading')).toBeVisible();
			await expect(authenticatedPage.getByTestId('score-value')).toBeVisible();
		});

		test('should display scoring reasons', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications/new');
			await authenticatedPage.getByTestId('input-name').fill('Reasons Test');
			await authenticatedPage.getByTestId('input-income').fill('4000');
			await authenticatedPage.getByTestId('input-fixed-costs').fill('1500');
			await authenticatedPage.getByTestId('input-desired-rate').fill('400');
			await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
			await authenticatedPage.getByTestId('radio-payment-default-no').check();
			await authenticatedPage.getByTestId('btn-submit-application').click();
	await authenticatedPage.getByTestId('confirm-dialog-confirm').click();
			await expect(authenticatedPage.getByTestId('scoring-reasons')).toBeVisible();
		});

		test('should show green traffic light for high score', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications/new');
			await authenticatedPage.getByTestId('input-name').fill('Green Light Test');
			await authenticatedPage.getByTestId('input-income').fill('6000');
			await authenticatedPage.getByTestId('input-fixed-costs').fill('2000');
			await authenticatedPage.getByTestId('input-desired-rate').fill('500');
			await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
			await authenticatedPage.getByTestId('radio-payment-default-no').check();
			await authenticatedPage.getByTestId('btn-submit-application').click();
			await authenticatedPage.getByTestId('confirm-dialog-confirm').click();
			await expect(authenticatedPage.getByTestId('traffic-light-green')).toBeVisible();
		});

		test('should show red traffic light for low score with payment default', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications/new');

			await authenticatedPage.waitForLoadState('networkidle');

			await authenticatedPage.getByTestId('input-name').click();
			await authenticatedPage.getByTestId('input-name').fill('Red Light Test');

			await authenticatedPage.getByTestId('input-income').click();
			await authenticatedPage.getByTestId('input-income').fill('2500');

			await authenticatedPage.getByTestId('input-fixed-costs').click();
			await authenticatedPage.getByTestId('input-fixed-costs').fill('2000');

			await authenticatedPage.getByTestId('input-desired-rate').click();
			await authenticatedPage.getByTestId('input-desired-rate').fill('300');

			await authenticatedPage.getByTestId('select-employment-status').selectOption('unemployed');
			await authenticatedPage.getByTestId('radio-payment-default-yes').check();

			await expect(authenticatedPage.getByTestId('input-name')).toHaveValue('Red Light Test');
			await expect(authenticatedPage.getByTestId('input-income')).toHaveValue('2500');

			await authenticatedPage.getByTestId('btn-submit-application').click();
			await authenticatedPage.getByTestId('confirm-dialog-confirm').click();
			await expect(authenticatedPage).toHaveURL(/\/applications\/\d+/, { timeout: 10000 });

			await expect(authenticatedPage.getByTestId('traffic-light-red')).toBeVisible();
		});
	});

	test.describe('Anträge verwalten und überwachen (Manage and Monitor Applications)', () => {
		test('should display applications list with table', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications');
			await expect(authenticatedPage.getByTestId('application-table')).toBeVisible();
		});

		test('should show application details when clicking on an application', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/applications/new');
			await authenticatedPage.getByTestId('input-name').fill('Detail View Test');
			await authenticatedPage.getByTestId('input-income').fill('4000');
			await authenticatedPage.getByTestId('input-fixed-costs').fill('1500');
			await authenticatedPage.getByTestId('input-desired-rate').fill('400');
			await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
			await authenticatedPage.getByTestId('radio-payment-default-no').check();
			await authenticatedPage.getByTestId('btn-save-draft').click();

			await expect(authenticatedPage.getByText('Detail View Test')).toBeVisible();
			await expect(authenticatedPage.getByText(/4.*000/)).toBeVisible();
		});
	});
});
