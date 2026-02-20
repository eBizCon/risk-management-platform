import { test, expect } from './fixtures';
import { createTestSession, clearTestSessions } from './helpers/auth';

test.describe('Antragsbearbeiter (Processor) Workflows', () => {
	test.describe('Eingereichte Anträge einsehen (View Submitted Applications)', () => {
		test.use({ userRole: 'processor' });

		test('should navigate to processor view', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/processor');
			await expect(authenticatedPage.locator('h1')).toContainText(/Anträge bearbeiten/i);
		});

		test('should display statistics overview', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/processor');
			await expect(authenticatedPage.getByText(/Gesamt|Total/i)).toBeVisible();
		});

		test('should show applications table', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/processor');
			await expect(authenticatedPage.locator('table, [role="table"]')).toBeVisible();
		});

		test('should filter applications by status', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/processor');

			const statusFilter = authenticatedPage.getByLabel(/Status/i);
			if (await statusFilter.isVisible()) {
				await statusFilter.selectOption('submitted');
				await expect(authenticatedPage).toHaveURL(/status=submitted/);
			}
		});
	});

	test.describe('Antragsdetails prüfen (Review Application Details)', () => {
		test.beforeEach(async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'applicant');

			await page.goto('/applications/new');
			await page.getByLabel(/Name/i).fill('Processor Review Test');
			await page.getByLabel(/Monatliches Einkommen/i).fill('4500');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('1800');
			await page.getByLabel(/Gewünschte Rate/i).fill('500');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();
			await page.getByRole('button', { name: /Antrag einreichen/i }).click();
			await page.getByTestId('confirm-dialog-confirm').click();

			await page.waitForURL(/\/applications\/\d+/);

			await clearTestSessions(page);
			await createTestSession(page, 'processor');
		});

		test('should display application details in processor view', async ({ page }) => {
			await page.goto('/processor');

			const viewButton = page.getByRole('button', { name: /Ansehen/i }).first();
			if (await viewButton.isVisible()) {
				await viewButton.click();
				await expect(page).toHaveURL(/\/processor\/\d+/);
			}
		});

		test('should show scoring information in detail view', async ({ page }) => {
			await page.goto('/processor');

			const viewButton = page.getByRole('button', { name: /Ansehen/i }).first();
			if (await viewButton.isVisible()) {
				await viewButton.click();
				await expect(page.getByRole('heading', { name: /Automatische Bewertung/i })).toBeVisible();
			}
		});

		test('should show financial details', async ({ page }) => {
			await page.goto('/processor');

			const viewButton = page.getByRole('button', { name: /Ansehen/i }).first();
			if (await viewButton.isVisible()) {
				await viewButton.click();
				await expect(page.getByRole('heading', { name: /Finanzielle Situation/i })).toBeVisible();
				await expect(page.getByText('Monatliches Einkommen')).toBeVisible();
			}
		});
	});

	test.describe('Antrag genehmigen (Approve Application)', () => {
		test('should show approve button for submitted applications', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'applicant');

			await page.goto('/applications/new');
			await page.getByLabel(/Name/i).fill('Approve Test');
			await page.getByLabel(/Monatliches Einkommen/i).fill('5000');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');
			await page.getByLabel(/Gewünschte Rate/i).fill('500');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();
			await page.getByRole('button', { name: /Antrag einreichen/i }).click();
			await page.getByTestId('confirm-dialog-confirm').click();

			const url = page.url();
			const match = url.match(/\/applications\/(\d+)/);
			const applicationId = match ? match[1] : null;

			await clearTestSessions(page);
			await createTestSession(page, 'processor');

			if (applicationId) {
				await page.goto(`/processor/${applicationId}`);
				await expect(page.getByText(/Genehmigen|Approve/i)).toBeVisible();
			}
		});

		test('should approve application with optional comment', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'applicant');

			await page.goto('/applications/new');
			await page.getByLabel(/Name/i).fill('Approve With Comment');
			await page.getByLabel(/Monatliches Einkommen/i).fill('5500');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');
			await page.getByLabel(/Gewünschte Rate/i).fill('500');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();
			await page.getByRole('button', { name: /Antrag einreichen/i }).click();
			await page.getByTestId('confirm-dialog-confirm').click();

			const url = page.url();
			const match = url.match(/\/applications\/(\d+)/);
			const applicationId = match ? match[1] : null;

			await clearTestSessions(page);
			await createTestSession(page, 'processor');

			if (applicationId) {
				await page.goto(`/processor/${applicationId}`);

				const approveRadio = page.getByLabel(/Genehmigen|Approve/i);
				if (await approveRadio.isVisible()) {
					await approveRadio.check();
				}

				const commentField = page.getByLabel(/Kommentar|Comment/i);
				if (await commentField.isVisible()) {
					await commentField.fill('Genehmigt aufgrund guter Bonität');
				}

				await page.getByRole('button', { name: /Entscheidung speichern|Absenden|Submit/i }).click();

				await expect(page.getByText(/Genehmigt|Approved/i)).toBeVisible();
			}
		});
	});

	test.describe('Antrag ablehnen (Reject Application)', () => {
		test('should require comment when rejecting', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'applicant');

			await page.goto('/applications/new');
			await page.getByLabel(/Name/i).fill('Reject Test');
			await page.getByLabel(/Monatliches Einkommen/i).fill('2500');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');
			await page.getByLabel(/Gewünschte Rate/i).fill('300');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('unemployed');
			await page.getByLabel(/Nein/i).check();
			await page.getByRole('button', { name: /Antrag einreichen/i }).click();
			await page.getByTestId('confirm-dialog-confirm').click();

			const url = page.url();
			const match = url.match(/\/applications\/(\d+)/);
			const applicationId = match ? match[1] : null;

			await clearTestSessions(page);
			await createTestSession(page, 'processor');

			if (applicationId) {
				await page.goto(`/processor/${applicationId}`);

				const rejectRadio = page.getByLabel(/Ablehnen|Reject/i);
				if (await rejectRadio.isVisible()) {
					await rejectRadio.check();
				}

				await page.getByRole('button', { name: /Entscheidung speichern|Absenden|Submit/i }).click();

				await expect(
					page.getByText(/Begründung.*erforderlich|Kommentar.*Pflicht|comment.*required/i)
				).toBeVisible();
			}
		});

		test('should reject application with required comment', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'applicant');

			await page.goto('/applications/new');
			await page.getByLabel(/Name/i).fill('Reject With Comment');
			await page.getByLabel(/Monatliches Einkommen/i).fill('2500');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');
			await page.getByLabel(/Gewünschte Rate/i).fill('300');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('unemployed');
			await page.getByLabel(/Ja/i).check();
			await page.getByRole('button', { name: /Antrag einreichen/i }).click();
			await page.getByTestId('confirm-dialog-confirm').click();

			const url = page.url();
			const match = url.match(/\/applications\/(\d+)/);
			const applicationId = match ? match[1] : null;

			await clearTestSessions(page);
			await createTestSession(page, 'processor');

			if (applicationId) {
				await page.goto(`/processor/${applicationId}`);

				const rejectRadio = page.getByLabel(/Ablehnen|Reject/i);
				if (await rejectRadio.isVisible()) {
					await rejectRadio.check();
				}

				const commentField = page.getByLabel(/Kommentar|Comment|Begründung/i);
				if (await commentField.isVisible()) {
					await commentField.fill('Abgelehnt wegen schlechter Bonität und Zahlungsverzug');
				}

				await page.getByRole('button', { name: /Entscheidung speichern|Absenden|Submit/i }).click();

				await expect(page.getByText(/Abgelehnt|Rejected/i)).toBeVisible();
			}
		});
	});

	test.describe('Bearbeitete Anträge nachverfolgen (Track Processed Applications)', () => {
		test.use({ userRole: 'processor' });

		test('should show approved applications in history', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/processor?status=approved');
			await expect(authenticatedPage.locator('table, [role="table"]')).toBeVisible();
		});

		test('should show rejected applications in history', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/processor?status=rejected');
			await expect(authenticatedPage.locator('table, [role="table"]')).toBeVisible();
		});

		test('should display processor comment in application detail', async ({ page }) => {
			await clearTestSessions(page);
			await createTestSession(page, 'applicant');

			await page.goto('/applications/new');
			await page.getByLabel(/Name/i).fill('Comment Display Test');
			await page.getByLabel(/Monatliches Einkommen/i).fill('5000');
			await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');
			await page.getByLabel(/Gewünschte Rate/i).fill('500');
			await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
			await page.getByLabel(/Nein/i).check();
			await page.getByRole('button', { name: /Antrag einreichen/i }).click();
			await page.getByTestId('confirm-dialog-confirm').click();

			const url = page.url();
			const match = url.match(/\/applications\/(\d+)/);
			const applicationId = match ? match[1] : null;

			await clearTestSessions(page);
			await createTestSession(page, 'processor');

			if (applicationId) {
				await page.goto(`/processor/${applicationId}`);

				const approveRadio = page.getByLabel(/Genehmigen|Approve/i);
				if (await approveRadio.isVisible()) {
					await approveRadio.check();
				}

				const commentField = page.getByLabel(/Kommentar|Comment/i);
				if (await commentField.isVisible()) {
					await commentField.fill('Test Kommentar für Anzeige');
				}

				await page.getByRole('button', { name: /Entscheidung speichern|Absenden|Submit/i }).click();

				await expect(page.getByText(/Test Kommentar für Anzeige/i)).toBeVisible();
			}
		});
	});

	test.describe('Ampel-Filter (Traffic Light Filter)', () => {
		test.use({ userRole: 'processor' });

		test('should display traffic light filter checkboxes', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/processor');

			const filterContainer = authenticatedPage.getByTestId('processor-traffic-light-filter');
			await expect(filterContainer).toBeVisible();
			await expect(authenticatedPage.getByTestId('processor-traffic-light-green')).toBeVisible();
			await expect(authenticatedPage.getByTestId('processor-traffic-light-yellow')).toBeVisible();
			await expect(authenticatedPage.getByTestId('processor-traffic-light-red')).toBeVisible();
		});

		test('should filter by single traffic light color', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/processor');

			await authenticatedPage.getByTestId('processor-traffic-light-red').check();
			await expect(authenticatedPage).toHaveURL(/trafficLight=red/);

			const rows = authenticatedPage.getByTestId(/^application-row-/);
			const count = await rows.count();
			if (count > 0) {
				for (let i = 0; i < count; i++) {
					const row = rows.nth(i);
					const redIndicator = row.getByTestId('traffic-light-red');
					await expect(redIndicator).toHaveClass(/traffic-red/);
				}
			}
		});

		test('should combine status and traffic light filters', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/processor');

			await authenticatedPage.getByTestId('processor-status-filter').selectOption('submitted');
			await expect(authenticatedPage).toHaveURL(/status=submitted/);

			await authenticatedPage.getByTestId('processor-traffic-light-red').check();
			await expect(authenticatedPage).toHaveURL(/trafficLight=red/);
			await expect(authenticatedPage).toHaveURL(/status=submitted/);
		});

		test('should remove filter when unchecking all colors', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/processor?trafficLight=red');

			await expect(authenticatedPage.getByTestId('processor-traffic-light-red')).toBeChecked();

			await authenticatedPage.getByTestId('processor-traffic-light-red').uncheck();
			await expect(authenticatedPage).not.toHaveURL(/trafficLight/);
		});

		test('should persist filter state in URL on reload', async ({ authenticatedPage }) => {
			await authenticatedPage.goto('/processor?trafficLight=green&trafficLight=yellow');

			await expect(authenticatedPage.getByTestId('processor-traffic-light-green')).toBeChecked();
			await expect(authenticatedPage.getByTestId('processor-traffic-light-yellow')).toBeChecked();
			await expect(authenticatedPage.getByTestId('processor-traffic-light-red')).not.toBeChecked();
		});
	});
});
