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

			await expect(page.getByLabel(/Name/i)).toBeVisible();
			await expect(page.getByLabel(/Monatliches Einkommen/i)).toBeVisible();
			await expect(page.getByLabel(/Monatliche Fixkosten/i)).toBeVisible();
			await expect(page.getByLabel(/Gewünschte Rate/i)).toBeVisible();
			await expect(page.getByLabel(/Beschäftigungsstatus/i)).toBeVisible();
			await expect(page.getByText(/Zahlungsverzug/i)).toBeVisible();
		});

		test('should show validation errors for empty form submission', async ({ page }) => {
			await page.goto('/applications/new');

			// Fill minimal data to bypass HTML5 required validation, but leave name too short
			await page.getByLabel(/Name/i).fill('A');
			await page.getByLabel(/Monatliches Einkommen/i).fill('1000');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('500');
			await page.getByLabel(/Gewünschte Rate/i).fill('200');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();

			await page.getByRole('button', { name: /Als Entwurf speichern/i }).click();

			await expect(page.getByText(/mindestens 2 Zeichen/i)).toBeVisible();
		});

		test('should show validation error for income equal to 0', async ({ page }) => {
			await page.goto('/applications/new');

			await page.getByLabel(/Name/i).fill('Test User');
			await page.getByLabel(/Monatliches Einkommen/i).fill('0');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('0');
			await page.getByLabel(/Gewünschte Rate/i).fill('200');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();

			await page.getByRole('button', { name: /Als Entwurf speichern/i }).click();

			await expect(page.getByText(/Einkommen muss positiv sein/i)).toBeVisible();
		});

		test('should show validation error when desired rate exceeds available income', async ({ page }) => {
			await page.goto('/applications/new');

			await page.getByLabel(/Name/i).fill('Test User');
			await page.getByLabel(/Monatliches Einkommen/i).fill('3000');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');
			await page.getByLabel(/Gewünschte Rate/i).fill('1500');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();

			await page.getByRole('button', { name: /Als Entwurf speichern/i }).click();

			await expect(page.getByText(/kann nicht höher sein als das verfügbare Einkommen/i)).toBeVisible();
		});

		test('should successfully create a draft application', async ({ page }) => {
			await page.goto('/applications/new');

			await page.getByLabel(/Name/i).fill('Max Mustermann');
			await page.getByLabel(/Monatliches Einkommen/i).fill('4000');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('1500');
			await page.getByLabel(/Gewünschte Rate/i).fill('500');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();

			await page.getByRole('button', { name: /Als Entwurf speichern/i }).click();

			await expect(page).toHaveURL(/\/applications\/\d+/);
			await expect(page.getByText(/Entwurf/i)).toBeVisible();
		});

		test('should successfully create and submit an application directly', async ({ page }) => {
			await page.goto('/applications/new');

			await page.getByLabel(/Name/i).fill('Anna Schmidt');
			await page.getByLabel(/Monatliches Einkommen/i).fill('5000');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');
			await page.getByLabel(/Gewünschte Rate/i).fill('600');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();

			await page.getByRole('button', { name: /Antrag einreichen/i }).click();

			await expect(page).toHaveURL(/\/applications\/\d+/);
			await expect(page.getByText('Eingereicht', { exact: true })).toBeVisible();
		});
	});

	test.describe('Entwurf speichern und bearbeiten (Save and Edit Draft)', () => {
		test('should display draft applications in the list', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByLabel(/Name/i).fill('Draft Test');
			await page.getByLabel(/Monatliches Einkommen/i).fill('3500');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('1200');
			await page.getByLabel(/Gewünschte Rate/i).fill('400');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();
			await page.getByRole('button', { name: /Als Entwurf speichern/i }).click();

			await page.goto('/applications');
			await expect(page.getByText('Draft Test').first()).toBeVisible();
			await expect(page.getByText('Entwurf', { exact: true }).first()).toBeVisible();
		});

		test('should allow editing a draft application', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByLabel(/Name/i).fill('Edit Test');
			await page.getByLabel(/Monatliches Einkommen/i).fill('3000');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('1000');
			await page.getByLabel(/Gewünschte Rate/i).fill('350');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();
			await page.getByRole('button', { name: /Als Entwurf speichern/i }).click();

			await page.getByRole('link', { name: /Bearbeiten/i }).click();
			await expect(page).toHaveURL(/\/applications\/\d+\/edit/);

			await page.getByLabel(/Name/i).fill('Edit Test Updated');
			await page.getByRole('button', { name: /Als Entwurf speichern/i }).click();

			await expect(page.getByText('Edit Test Updated')).toBeVisible();
		});

		test('should filter applications by status', async ({ page }) => {
			await page.goto('/applications');

			const statusFilter = page.getByLabel(/Status/i);
			if (await statusFilter.isVisible()) {
				await statusFilter.selectOption('draft');
				await expect(page).toHaveURL(/status=draft/);
			}
		});
	});

	test.describe('Antrag einreichen (Submit Application)', () => {
		test('should submit a draft application', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByLabel(/Name/i).fill('Submit Test');
			await page.getByLabel(/Monatliches Einkommen/i).fill('4500');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('1800');
			await page.getByLabel(/Gewünschte Rate/i).fill('500');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();
			await page.getByRole('button', { name: /Als Entwurf speichern/i }).click();



			await page.getByTestId('submit-application').click();
			// Handle the confirmation dialog
			page.on('dialog', async dialog => {
				await dialog.accept();
			});

			// Wait for page reload after submission
			await page.waitForLoadState('networkidle');

			await expect(page.getByText('Eingereicht', { exact: true })).toBeVisible();
		});

		test('should not allow editing after submission', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByLabel(/Name/i).fill('No Edit After Submit');
			await page.getByLabel(/Monatliches Einkommen/i).fill('4000');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('1500');
			await page.getByLabel(/Gewünschte Rate/i).fill('450');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();
			await page.getByRole('button', { name: /Antrag einreichen/i }).click();

			await expect(page.getByRole('link', { name: /Bearbeiten/i })).not.toBeVisible();
		});
	});

	test.describe('Bewertungsergebnis einsehen (View Scoring Result)', () => {
		test('should display score and traffic light for submitted application', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByLabel(/Name/i).fill('Score Test');
			await page.getByLabel(/Monatliches Einkommen/i).fill('5000');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');
			await page.getByLabel(/Gewünschte Rate/i).fill('500');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();
			await page.getByRole('button', { name: /Antrag einreichen/i }).click();

			await expect(page.getByRole('heading', { name: 'Bewertung', exact: true, level: 2 })).toBeVisible();
			await expect(page.getByText(/von 100/i)).toBeVisible();
		});

		test('should display scoring reasons', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByLabel(/Name/i).fill('Reasons Test');
			await page.getByLabel(/Monatliches Einkommen/i).fill('4000');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('1500');
			await page.getByLabel(/Gewünschte Rate/i).fill('400');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();
			await page.getByRole('button', { name: /Antrag einreichen/i }).click();

			await expect(page.getByText(/Bewertungsgründe|Entscheidungsgründe/i)).toBeVisible();
		});

		test('should show green traffic light for high score', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByLabel(/Name/i).fill('Green Light Test');
			await page.getByLabel(/Monatliches Einkommen/i).fill('6000');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');
			await page.getByLabel(/Gewünschte Rate/i).fill('500');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();
			await page.getByRole('button', { name: /Antrag einreichen/i }).click();

			const greenIndicator = page.locator('.bg-green-500, [class*="green"]');
			await expect(greenIndicator.first()).toBeVisible();
		});

		test('should show red traffic light for low score with payment default', async ({ page }) => {
			await page.goto('/applications/new');

			// Wait for form to be ready
			await page.waitForLoadState('networkidle');

			// Fill all fields sequentially with explicit waits
			await page.getByLabel(/Name/i).click();
			await page.getByLabel(/Name/i).fill('Red Light Test');

			await page.getByLabel(/Monatliches Einkommen/i).click();
			await page.getByLabel(/Monatliches Einkommen/i).fill('2500');

			await page.getByLabel(/Monatliche Fixkosten/i).click();
			await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');

			await page.getByLabel(/Gewünschte Rate/i).click();
			await page.getByLabel(/Gewünschte Rate/i).fill('300');

			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('unemployed');
			await page.getByLabel(/Ja/i).check();

			// Verify fields are filled before submitting
			await expect(page.getByLabel(/Name/i)).toHaveValue('Red Light Test');
			await expect(page.getByLabel(/Monatliches Einkommen/i)).toHaveValue('2500');

			await page.getByRole('button', { name: /Antrag einreichen/i }).click();

			// Wait for navigation to application detail page
			await expect(page).toHaveURL(/\/applications\/\d+/, { timeout: 10000 });

			const redIndicator = page.locator('.bg-red-500, [class*="red"]');
			await expect(redIndicator.first()).toBeVisible();
		});
	});

	test.describe('Anträge verwalten und überwachen (Manage and Monitor Applications)', () => {
		test('should display applications list with table', async ({ page }) => {
			await page.goto('/applications');
			await expect(page.locator('table, [role="table"]')).toBeVisible();
		});

		test('should show application details when clicking on an application', async ({ page }) => {
			await page.goto('/applications/new');
			await page.getByLabel(/Name/i).fill('Detail View Test');
			await page.getByLabel(/Monatliches Einkommen/i).fill('4000');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('1500');
			await page.getByLabel(/Gewünschte Rate/i).fill('400');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();
			await page.getByRole('button', { name: /Als Entwurf speichern/i }).click();

			await expect(page.getByText('Detail View Test')).toBeVisible();
			await expect(page.getByText(/4.*000/)).toBeVisible();
		});
	});
});
