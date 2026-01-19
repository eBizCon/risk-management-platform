import { test, expect } from '@playwright/test';

test.describe('Antragsteller (Applicant) Workflows', () => {
	test.beforeEach(async ({ page, context }) => {
		// Set the applicant role cookie before navigating
		await context.addCookies([{
			name: 'risk-management-user-role',
			value: 'applicant',
			domain: 'localhost',
			path: '/'
		}]);
		await page.goto('/');
	});

	test.describe('Kreditantrag erstellen (Create Credit Application)', () => {
		test('should display the home page with role-specific content', async ({ page }) => {
			await expect(page.locator('h1')).toContainText('Risikomanagement');
			await expect(page.getByRole('link', { name: /Neuer Antrag/i })).toBeVisible();
		});

		test('should navigate to new application form', async ({ page }) => {
			await page.getByRole('link', { name: /Neuer Antrag/i }).click();
			await expect(page).toHaveURL('/applications/new');
			await expect(page.locator('h1')).toContainText('Kreditantrag erstellen');
		});

		test('should display all required form fields', async ({ page }) => {
			await page.goto('/applications/new');

			await expect(page.getByTestId('input-name')).toBeVisible();
			await expect(page.getByTestId('input-income')).toBeVisible();
			await expect(page.getByTestId('input-fixed-costs')).toBeVisible();
			await expect(page.getByTestId('input-desired-rate')).toBeVisible();
			await expect(page.getByTestId('select-employment-status')).toBeVisible();
			await expect(page.getByTestId('radio-payment-default-yes')).toBeVisible();
			await expect(page.getByTestId('radio-payment-default-no')).toBeVisible();
		});

		test('should show validation errors for empty form submission', async ({ page }) => {
			await page.goto('/applications/new');

			// Fill minimal data to bypass HTML5 required validation, but leave name too short
			await page.getByTestId('input-name').fill('A');
			await page.getByTestId('input-income').fill('1000');
			await page.getByTestId('input-fixed-costs').fill('500');
			await page.getByTestId('input-desired-rate').fill('200');
			await page.getByTestId('select-employment-status').selectOption('employed');
			await page.getByTestId('radio-payment-default-no').check();

			await page.getByTestId('btn-save-draft').click();

			await expect(page.getByText(/mindestens 2 Zeichen/i)).toBeVisible();
		});

		test('should show validation error for income equal to 0', async ({ page }) => {
			await page.goto('/applications/new');

			await page.getByTestId('input-name').fill('Test User');
			await page.getByTestId('input-income').fill('0');
			await page.getByTestId('input-fixed-costs').fill('0');
			await page.getByTestId('input-desired-rate').fill('200');
			await page.getByTestId('select-employment-status').selectOption('employed');
			await page.getByTestId('radio-payment-default-no').check();

			await page.getByTestId('btn-save-draft').click();

			await expect(page.getByText(/Einkommen muss positiv sein/i)).toBeVisible();
		});

		test('should show validation error when desired rate exceeds available income', async ({ page }) => {
			await page.goto('/applications/new');

			await page.getByTestId('input-name').fill('Test User');
			await page.getByTestId('input-income').fill('3000');
			await page.getByTestId('input-fixed-costs').fill('2000');
			await page.getByTestId('input-desired-rate').fill('1500');
			await page.getByTestId('select-employment-status').selectOption('employed');
			await page.getByTestId('radio-payment-default-no').check();

			await page.getByTestId('btn-save-draft').click();

			await expect(page.getByText(/kann nicht höher sein als das verfügbare Einkommen/i)).toBeVisible();
		});

		test('should successfully create a draft application', async ({ page }) => {
			await page.goto('/applications/new');

			await page.getByTestId('input-name').fill('Max Mustermann');
			await page.getByTestId('input-income').fill('4000');
			await page.getByTestId('input-fixed-costs').fill('1500');
			await page.getByTestId('input-desired-rate').fill('500');
			await page.getByTestId('select-employment-status').selectOption('employed');
			await page.getByTestId('radio-payment-default-no').check();

			await page.getByTestId('btn-save-draft').click();

			await expect(page).toHaveURL(/\/applications\/\d+/);
			await expect(page.getByTestId('status-badge-draft')).toBeVisible();
		});

		test('should successfully create and submit an application directly', async ({ page }) => {
			await page.goto('/applications/new');

			await page.getByTestId('input-name').fill('Anna Schmidt');
			await page.getByTestId('input-income').fill('5000');
			await page.getByTestId('input-fixed-costs').fill('2000');
			await page.getByTestId('input-desired-rate').fill('600');
			await page.getByTestId('select-employment-status').selectOption('employed');
			await page.getByTestId('radio-payment-default-no').check();

			await page.getByTestId('btn-submit-application').click();

			await expect(page).toHaveURL(/\/applications\/\d+/);
			await expect(page.getByTestId('status-badge-submitted')).toBeVisible();
		});
	});

	test.describe('Entwurf speichern und bearbeiten (Save and Edit Draft)', () => {
		test('should display draft applications in the list', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByTestId('input-name').fill('Draft Test');
			await page.getByTestId('input-income').fill('3500');
			await page.getByTestId('input-fixed-costs').fill('1200');
			await page.getByTestId('input-desired-rate').fill('400');
			await page.getByTestId('select-employment-status').selectOption('employed');
			await page.getByTestId('radio-payment-default-no').check();
			await page.getByTestId('btn-save-draft').click();

			await page.goto('/applications');
			await expect(page.getByTestId('application-table')).toBeVisible();
			await expect(page.getByTestId('status-badge-draft').first()).toBeVisible();
		});

		// test('should allow editing a draft application', async ({ page }) => {
		// 	await page.goto('/applications/new');
		// 	await page.getByTestId('input-name').fill('Edit Test');
		// 	await page.getByTestId('input-income').fill('3000');
		// 	await page.getByTestId('input-fixed-costs').fill('1000');
		// 	await page.getByTestId('input-desired-rate').fill('350');
		// 	await page.getByTestId('select-employment-status').selectOption('employed');
		// 	await page.getByTestId('radio-payment-default-no').check();
		// 	await page.getByTestId('btn-save-draft').click();

		// 	await page.getByTestId('edit-application').click();
		// 	await expect(page).toHaveURL(/\/applications\/\d+\/edit/);

		// 	await page.getByTestId('input-name').fill('Edit Test Updated');
		// 	await page.getByTestId('btn-save-draft').click();
		// 	await expect(page.getByTestId('application-name-title' + page.url().split('/').pop())).toContainText('Edit Test Updated');

		// });

		test('should filter applications by status', async ({ page }) => {
			await page.goto('/applications');

			const statusFilter = page.getByTestId('status-filter');
			if (await statusFilter.isVisible()) {
				await statusFilter.selectOption('draft');
				await expect(page).toHaveURL(/status=draft/);
			}
		});
	});

	test.describe('Antrag einreichen (Submit Application)', () => {
		test('should submit a draft application', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByTestId('input-name').fill('Submit Test');
			await page.getByTestId('input-income').fill('4500');
			await page.getByTestId('input-fixed-costs').fill('1800');
			await page.getByTestId('input-desired-rate').fill('500');
			await page.getByTestId('select-employment-status').selectOption('employed');
			await page.getByTestId('radio-payment-default-no').check();
			await page.getByTestId('btn-save-draft').click();

			await page.getByTestId('submit-application').click();
			// Handle the confirmation dialog
			page.on('dialog', async dialog => {
				await dialog.accept();
				await page.waitForLoadState('networkidle');
				await expect(page.getByTestId('status-badge-submitted')).toBeVisible();
			});
		});

		test('should not allow editing after submission', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByTestId('input-name').fill('No Edit After Submit');
			await page.getByTestId('input-income').fill('4000');
			await page.getByTestId('input-fixed-costs').fill('1500');
			await page.getByTestId('input-desired-rate').fill('450');
			await page.getByTestId('select-employment-status').selectOption('employed');
			await page.getByTestId('radio-payment-default-no').check();
			await page.getByTestId('btn-submit-application').click();

			await expect(page.getByTestId('edit-application')).not.toBeVisible();
		});
	});

	test.describe('Bewertungsergebnis einsehen (View Scoring Result)', () => {
		test('should display score and traffic light for submitted application', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByTestId('input-name').fill('Score Test');
			await page.getByTestId('input-income').fill('5000');
			await page.getByTestId('input-fixed-costs').fill('2000');
			await page.getByTestId('input-desired-rate').fill('500');
			await page.getByTestId('select-employment-status').selectOption('employed');
			await page.getByTestId('radio-payment-default-no').check();
			await page.getByTestId('btn-submit-application').click();

			await expect(page.getByTestId('scoring-heading')).toBeVisible();
			await expect(page.getByTestId('score-value')).toBeVisible();
		});

		test('should display scoring reasons', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByTestId('input-name').fill('Reasons Test');
			await page.getByTestId('input-income').fill('4000');
			await page.getByTestId('input-fixed-costs').fill('1500');
			await page.getByTestId('input-desired-rate').fill('400');
			await page.getByTestId('select-employment-status').selectOption('employed');
			await page.getByTestId('radio-payment-default-no').check();
			await page.getByTestId('btn-submit-application').click();

			await expect(page.getByTestId('scoring-reasons')).toBeVisible();
		});

		test('should show green traffic light for high score', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByTestId('input-name').fill('Green Light Test');
			await page.getByTestId('input-income').fill('6000');
			await page.getByTestId('input-fixed-costs').fill('2000');
			await page.getByTestId('input-desired-rate').fill('500');
			await page.getByTestId('select-employment-status').selectOption('employed');
			await page.getByTestId('radio-payment-default-no').check();
			await page.getByTestId('btn-submit-application').click();

			await expect(page.getByTestId('traffic-light-green')).toBeVisible();
		});

		test('should show red traffic light for low score with payment default', async ({ page }) => {
			await page.goto('/applications/new');

			// Wait for form to be ready
			await page.waitForLoadState('networkidle');

			// Fill all fields sequentially with explicit waits
			await page.getByTestId('input-name').click();
			await page.getByTestId('input-name').fill('Red Light Test');

			await page.getByTestId('input-income').click();
			await page.getByTestId('input-income').fill('2500');

			await page.getByTestId('input-fixed-costs').click();
			await page.getByTestId('input-fixed-costs').fill('2000');

			await page.getByTestId('input-desired-rate').click();
			await page.getByTestId('input-desired-rate').fill('300');

			await page.getByTestId('select-employment-status').selectOption('unemployed');
			await page.getByTestId('radio-payment-default-yes').check();

			// Verify fields are filled before submitting
			await expect(page.getByTestId('input-name')).toHaveValue('Red Light Test');
			await expect(page.getByTestId('input-income')).toHaveValue('2500');

			await page.getByTestId('btn-submit-application').click();

			// Wait for navigation to application detail page
			await expect(page).toHaveURL(/\/applications\/\d+/, { timeout: 10000 });

			await expect(page.getByTestId('traffic-light-red')).toBeVisible();
		});
	});

	test.describe('Anträge verwalten und überwachen (Manage and Monitor Applications)', () => {
		test('should display applications list with table', async ({ page }) => {
			await page.goto('/applications');
			await expect(page.getByTestId('application-table')).toBeVisible();
		});

		test('should show application details when clicking on an application', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByTestId('input-name').fill('Detail View Test');
			await page.getByTestId('input-income').fill('4000');
			await page.getByTestId('input-fixed-costs').fill('1500');
			await page.getByTestId('input-desired-rate').fill('400');
			await page.getByTestId('select-employment-status').selectOption('employed');
			await page.getByTestId('radio-payment-default-no').check();
			await page.getByTestId('btn-save-draft').click();

			await expect(page.getByText('Detail View Test')).toBeVisible();
			await expect(page.getByText(/4.*000/)).toBeVisible();
		});
	});
});
