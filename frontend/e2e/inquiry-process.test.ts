import { test, expect } from './fixtures';
import { clearTestSessions, createTestSession } from './helpers/auth';

async function createSubmittedApplication(page: import('@playwright/test').Page, name: string) {
	await page.goto('/applications/new');
	await page.getByTestId('input-name').fill(name);
	await page.getByTestId('input-income').fill('4500');
	await page.getByTestId('input-fixed-costs').fill('1800');
	await page.getByTestId('input-desired-rate').fill('500');
	await page.getByTestId('select-employment-status').selectOption('employed');
	await page.getByTestId('radio-payment-default-no').check();
	await page.getByTestId('btn-submit-application').click();
	await page.getByTestId('confirm-dialog-confirm').click();
	await page.waitForURL(/\/applications\/\d+/);
	const match = page.url().match(/\/applications\/(\d+)/);
	if (!match) {
		throw new Error('Application id not found in URL');
	}
	return Number.parseInt(match[1], 10);
}

test.describe('Inquiry process', () => {
	test('processor can create inquiry and applicant can answer it', async ({ page }) => {
		await clearTestSessions(page);
		await createTestSession(page, 'applicant');
		const applicationId = await createSubmittedApplication(page, 'Inquiry Flow Test');

		await clearTestSessions(page);
		await createTestSession(page, 'processor');
		await page.goto(`/processor/${applicationId}`);
		await page
			.getByTestId('processor-inquiry-input')
			.fill('Bitte erläutern Sie die erhöhten Fixkosten.');
		await page.getByTestId('processor-inquiry-submit').click();
		await expect(page.getByTestId('processor-inquiry-created-message')).toBeVisible();
		await expect(page.getByTestId(`status-badge-needs_information`)).toBeVisible();

		await clearTestSessions(page);
		await createTestSession(page, 'applicant');
		await page.goto(`/applications/${applicationId}`);
		await expect(page.getByTestId('application-inquiry-history')).toBeVisible();
		await page
			.getByTestId('application-inquiry-response-input')
			.fill('Die höheren Fixkosten stammen aus einer vorübergehenden Doppelbelastung.');
		await page.getByTestId('application-inquiry-response-submit').click();
		await expect(page.getByTestId('application-inquiry-answered-message')).toBeVisible();
		await expect(page.getByTestId('status-badge-resubmitted')).toBeVisible();
		await expect(page.getByTestId('application-inquiry-response')).toBeVisible();
	});

	test('processor can decide a resubmitted application after applicant response', async ({
		page
	}) => {
		await clearTestSessions(page);
		await createTestSession(page, 'applicant');
		const applicationId = await createSubmittedApplication(page, 'Resubmitted Decision Test');

		await clearTestSessions(page);
		await createTestSession(page, 'processor');
		await page.goto(`/processor/${applicationId}`);
		await page
			.getByTestId('processor-inquiry-input')
			.fill('Bitte erläutern Sie die erhöhten Fixkosten.');
		await page.getByTestId('processor-inquiry-submit').click();
		await expect(page.getByTestId('status-badge-needs_information')).toBeVisible();

		await clearTestSessions(page);
		await createTestSession(page, 'applicant');
		await page.goto(`/applications/${applicationId}`);
		await page
			.getByTestId('application-inquiry-response-input')
			.fill('Die Fixkosten waren einmalig erhöht und normalisieren sich ab dem nächsten Monat.');
		await page.getByTestId('application-inquiry-response-submit').click();
		await expect(page.getByTestId('status-badge-resubmitted')).toBeVisible();

		await clearTestSessions(page);
		await createTestSession(page, 'processor');
		await page.goto(`/processor/${applicationId}`);
		await expect(page.getByTestId('status-badge-resubmitted')).toBeVisible();
		await page.getByLabel(/Genehmigen/i).check();
		await page.getByRole('button', { name: /Antrag genehmigen|Entscheidung treffen/i }).click();
		await expect(page.getByTestId('status-badge-approved')).toBeVisible();
	});

	test('applicant cannot answer inquiry for foreign application', async ({ page, browser }) => {
		await clearTestSessions(page);
		await createTestSession(page, 'applicant', { email: 'owner@example.com', name: 'Owner' });
		const applicationId = await createSubmittedApplication(page, 'Foreign Inquiry Test');

		await clearTestSessions(page);
		await createTestSession(page, 'processor');
		await page.goto(`/processor/${applicationId}`);
		await page
			.getByTestId('processor-inquiry-input')
			.fill('Bitte reichen Sie zusätzliche Informationen ein.');
		await page.getByTestId('processor-inquiry-submit').click();
		await expect(page.getByTestId('processor-inquiry-created-message')).toBeVisible();

		const otherContext = await browser.newContext();
		const otherPage = await otherContext.newPage();
		await createTestSession(otherPage, 'applicant', {
			email: 'other@example.com',
			name: 'Other Applicant'
		});

		const response = await otherPage.request.post(
			`/api/applications/${applicationId}/inquiry/response`,
			{
				data: { responseText: 'Unauthorized attempt' },
				headers: { 'Content-Type': 'application/json' }
			}
		);

		expect(response.status()).toBe(403);
		await clearTestSessions(otherPage);
		await otherContext.close();
	});

	test('unauthenticated inquiry api access returns 401', async ({ page }) => {
		const response = await page.request.post('/api/applications/999999/inquiry', {
			data: { inquiryText: 'Missing authentication' },
			headers: { 'Content-Type': 'application/json' }
		});

		expect(response.status()).toBe(401);
	});

	test('wrong-role inquiry api access returns 403', async ({ page }) => {
		await clearTestSessions(page);
		await createTestSession(page, 'applicant');

		const response = await page.request.post('/api/applications/999999/inquiry', {
			data: { inquiryText: 'Wrong role' },
			headers: { 'Content-Type': 'application/json' }
		});

		expect(response.status()).toBe(403);
	});
});
