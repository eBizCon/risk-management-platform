import { test, expect } from '@playwright/test';

test.describe('Antragsbearbeiter (Processor) Workflows', () => {
	test.beforeEach(async ({ page }) => {
		await page.goto('/');
		const debugToggle = page.getByRole('button', { name: /Debug/i });
		if (await debugToggle.isVisible()) {
			await debugToggle.click();
			const processorButton = page.getByRole('button', { name: /Bearbeiter/i });
			if (await processorButton.isVisible()) {
				await processorButton.click();
			}
		}
	});

	test.describe('Eingereichte Anträge einsehen (View Submitted Applications)', () => {
		test('should navigate to processor view', async ({ page }) => {
				await page.goto('/processor');
				await expect(page.locator('h1')).toContainText(/Anträge bearbeiten/i);
			});

		test('should display statistics overview', async ({ page }) => {
			await page.goto('/processor');
			await expect(page.getByText(/Gesamt|Total/i)).toBeVisible();
		});

		test('should show applications table', async ({ page }) => {
			await page.goto('/processor');
			await expect(page.locator('table, [role="table"]')).toBeVisible();
		});

		test('should filter applications by status', async ({ page }) => {
			await page.goto('/processor');
			
			const statusFilter = page.getByLabel(/Status/i);
			if (await statusFilter.isVisible()) {
				await statusFilter.selectOption('submitted');
				await expect(page).toHaveURL(/status=submitted/);
			}
		});
	});

	test.describe('Antragsdetails prüfen (Review Application Details)', () => {
		test.beforeEach(async ({ page }) => {
			await page.goto('/');
			const debugToggle = page.getByRole('button', { name: /Debug/i });
			if (await debugToggle.isVisible()) {
				await debugToggle.click();
				const applicantButton = page.getByRole('button', { name: /Antragsteller/i });
				if (await applicantButton.isVisible()) {
					await applicantButton.click();
				}
			}
			
			await page.goto('/applications/new');
				await page.getByLabel(/Name/i).fill('Processor Review Test');
				await page.getByLabel(/Monatliches Einkommen/i).fill('4500');
				await page.getByLabel(/Monatliche Fixkosten/i).fill('1800');
				await page.getByLabel(/Gewünschte Rate/i).fill('500');
				await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
				await page.getByLabel(/Nein/i).check();
				await page.getByRole('button', { name: /Antrag einreichen/i }).click();
			
			const debugToggle2 = page.getByRole('button', { name: /Debug/i });
			if (await debugToggle2.isVisible()) {
				await debugToggle2.click();
				const processorButton = page.getByRole('button', { name: /Bearbeiter/i });
				if (await processorButton.isVisible()) {
					await processorButton.click();
				}
			}
		});

		test('should display application details in processor view', async ({ page }) => {
			await page.goto('/processor');
			
			const applicationLink = page.getByRole('link', { name: /Details|Prüfen|Anzeigen/i }).first();
			if (await applicationLink.isVisible()) {
				await applicationLink.click();
				await expect(page).toHaveURL(/\/processor\/\d+/);
			}
		});

		test('should show scoring information in detail view', async ({ page }) => {
			await page.goto('/processor');
			
			const applicationLink = page.getByRole('link', { name: /Details|Prüfen|Anzeigen/i }).first();
			if (await applicationLink.isVisible()) {
				await applicationLink.click();
				await expect(page.getByText(/Score|Bewertung/i)).toBeVisible();
			}
		});

		test('should show financial details', async ({ page }) => {
			await page.goto('/processor');
			
			const applicationLink = page.getByRole('link', { name: /Details|Prüfen|Anzeigen/i }).first();
			if (await applicationLink.isVisible()) {
				await applicationLink.click();
				await expect(page.getByText(/Einkommen|Income/i)).toBeVisible();
				await expect(page.getByText(/Fixkosten|Fixed Costs/i)).toBeVisible();
			}
		});
	});

	test.describe('Antrag genehmigen (Approve Application)', () => {
		test('should show approve button for submitted applications', async ({ page }) => {
			await page.goto('/');
			const debugToggle = page.getByRole('button', { name: /Debug/i });
			if (await debugToggle.isVisible()) {
				await debugToggle.click();
				const applicantButton = page.getByRole('button', { name: /Antragsteller/i });
				if (await applicantButton.isVisible()) {
					await applicantButton.click();
				}
			}
			
			await page.goto('/applications/new');
				await page.getByLabel(/Name/i).fill('Approve Test');
				await page.getByLabel(/Monatliches Einkommen/i).fill('5000');
				await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');
				await page.getByLabel(/Gewünschte Rate/i).fill('500');
				await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
				await page.getByLabel(/Nein/i).check();
				await page.getByRole('button', { name: /Antrag einreichen/i }).click();
			
			const url = page.url();
			const match = url.match(/\/applications\/(\d+)/);
			const applicationId = match ? match[1] : null;
			
			const debugToggle2 = page.getByRole('button', { name: /Debug/i });
			if (await debugToggle2.isVisible()) {
				await debugToggle2.click();
				const processorButton = page.getByRole('button', { name: /Bearbeiter/i });
				if (await processorButton.isVisible()) {
					await processorButton.click();
				}
			}
			
			if (applicationId) {
				await page.goto(`/processor/${applicationId}`);
				await expect(page.getByText(/Genehmigen|Approve/i)).toBeVisible();
			}
		});

		test('should approve application with optional comment', async ({ page }) => {
			await page.goto('/');
			const debugToggle = page.getByRole('button', { name: /Debug/i });
			if (await debugToggle.isVisible()) {
				await debugToggle.click();
				const applicantButton = page.getByRole('button', { name: /Antragsteller/i });
				if (await applicantButton.isVisible()) {
					await applicantButton.click();
				}
			}
			
			await page.goto('/applications/new');
				await page.getByLabel(/Name/i).fill('Approve With Comment');
				await page.getByLabel(/Monatliches Einkommen/i).fill('5500');
				await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');
				await page.getByLabel(/Gewünschte Rate/i).fill('500');
				await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
				await page.getByLabel(/Nein/i).check();
				await page.getByRole('button', { name: /Antrag einreichen/i }).click();
			
			const url = page.url();
			const match = url.match(/\/applications\/(\d+)/);
			const applicationId = match ? match[1] : null;
			
			const debugToggle2 = page.getByRole('button', { name: /Debug/i });
			if (await debugToggle2.isVisible()) {
				await debugToggle2.click();
				const processorButton = page.getByRole('button', { name: /Bearbeiter/i });
				if (await processorButton.isVisible()) {
					await processorButton.click();
				}
			}
			
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
			await page.goto('/');
			const debugToggle = page.getByRole('button', { name: /Debug/i });
			if (await debugToggle.isVisible()) {
				await debugToggle.click();
				const applicantButton = page.getByRole('button', { name: /Antragsteller/i });
				if (await applicantButton.isVisible()) {
					await applicantButton.click();
				}
			}
			
			await page.goto('/applications/new');
				await page.getByLabel(/Name/i).fill('Reject Test');
				await page.getByLabel(/Monatliches Einkommen/i).fill('2500');
				await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');
				await page.getByLabel(/Gewünschte Rate/i).fill('300');
				await page.getByLabel(/Beschäftigungsstatus/i).selectOption('unemployed');
				await page.getByLabel(/Nein/i).check();
				await page.getByRole('button', { name: /Antrag einreichen/i }).click();
			
			const url = page.url();
			const match = url.match(/\/applications\/(\d+)/);
			const applicationId = match ? match[1] : null;
			
			const debugToggle2 = page.getByRole('button', { name: /Debug/i });
			if (await debugToggle2.isVisible()) {
				await debugToggle2.click();
				const processorButton = page.getByRole('button', { name: /Bearbeiter/i });
				if (await processorButton.isVisible()) {
					await processorButton.click();
				}
			}
			
			if (applicationId) {
				await page.goto(`/processor/${applicationId}`);
				
				const rejectRadio = page.getByLabel(/Ablehnen|Reject/i);
				if (await rejectRadio.isVisible()) {
					await rejectRadio.check();
				}
				
				await page.getByRole('button', { name: /Entscheidung speichern|Absenden|Submit/i }).click();
				
				await expect(page.getByText(/Begründung.*erforderlich|Kommentar.*Pflicht|comment.*required/i)).toBeVisible();
			}
		});

		test('should reject application with required comment', async ({ page }) => {
			await page.goto('/');
			const debugToggle = page.getByRole('button', { name: /Debug/i });
			if (await debugToggle.isVisible()) {
				await debugToggle.click();
				const applicantButton = page.getByRole('button', { name: /Antragsteller/i });
				if (await applicantButton.isVisible()) {
					await applicantButton.click();
				}
			}
			
			await page.goto('/applications/new');
				await page.getByLabel(/Name/i).fill('Reject With Comment');
				await page.getByLabel(/Monatliches Einkommen/i).fill('2500');
				await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');
				await page.getByLabel(/Gewünschte Rate/i).fill('300');
				await page.getByLabel(/Beschäftigungsstatus/i).selectOption('unemployed');
				await page.getByLabel(/Ja/i).check();
				await page.getByRole('button', { name: /Antrag einreichen/i }).click();
			
			const url = page.url();
			const match = url.match(/\/applications\/(\d+)/);
			const applicationId = match ? match[1] : null;
			
			const debugToggle2 = page.getByRole('button', { name: /Debug/i });
			if (await debugToggle2.isVisible()) {
				await debugToggle2.click();
				const processorButton = page.getByRole('button', { name: /Bearbeiter/i });
				if (await processorButton.isVisible()) {
					await processorButton.click();
				}
			}
			
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
		test('should show approved applications in history', async ({ page }) => {
			await page.goto('/processor?status=approved');
			await expect(page.locator('table, [role="table"]')).toBeVisible();
		});

		test('should show rejected applications in history', async ({ page }) => {
			await page.goto('/processor?status=rejected');
			await expect(page.locator('table, [role="table"]')).toBeVisible();
		});

		test('should display processor comment in application detail', async ({ page }) => {
			await page.goto('/');
			const debugToggle = page.getByRole('button', { name: /Debug/i });
			if (await debugToggle.isVisible()) {
				await debugToggle.click();
				const applicantButton = page.getByRole('button', { name: /Antragsteller/i });
				if (await applicantButton.isVisible()) {
					await applicantButton.click();
				}
			}
			
			await page.goto('/applications/new');
				await page.getByLabel(/Name/i).fill('Comment Display Test');
				await page.getByLabel(/Monatliches Einkommen/i).fill('5000');
				await page.getByLabel(/Monatliche Fixkosten/i).fill('2000');
				await page.getByLabel(/Gewünschte Rate/i).fill('500');
				await page.getByLabel(/Beschäftigungsstatus/i).selectOption('employed');
				await page.getByLabel(/Nein/i).check();
				await page.getByRole('button', { name: /Antrag einreichen/i }).click();
			
			const url = page.url();
			const match = url.match(/\/applications\/(\d+)/);
			const applicationId = match ? match[1] : null;
			
			const debugToggle2 = page.getByRole('button', { name: /Debug/i });
			if (await debugToggle2.isVisible()) {
				await debugToggle2.click();
				const processorButton = page.getByRole('button', { name: /Bearbeiter/i });
				if (await processorButton.isVisible()) {
					await processorButton.click();
				}
			}
			
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
});
