import { test, expect } from './fixtures';

test.describe('Applicant CSV Export', () => {

	test('should display export button on applications page', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/applications');
		await expect(authenticatedPage.getByTestId('applications-export-button')).toBeVisible();
		await expect(authenticatedPage.getByTestId('applications-export-button')).toContainText('Als CSV exportieren');
	});

	test('should download CSV file when clicking export button', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/applications/new');
		await authenticatedPage.getByTestId('input-name').fill('CSV Export Test');
		await authenticatedPage.getByTestId('input-income').fill('4000');
		await authenticatedPage.getByTestId('input-fixed-costs').fill('1500');
		await authenticatedPage.getByTestId('input-desired-rate').fill('500');
		await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
		await authenticatedPage.getByTestId('radio-payment-default-no').check();
		await authenticatedPage.getByTestId('btn-save-draft').click();
		await authenticatedPage.waitForURL(/\/applications\/\d+/);

		await authenticatedPage.goto('/applications');

		const downloadPromise = authenticatedPage.waitForEvent('download');
		await authenticatedPage.getByTestId('applications-export-button').click();
		const download = await downloadPromise;

		expect(download.suggestedFilename()).toBe('meine-antraege.csv');
	});

	test('should contain correct CSV headers', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/applications/new');
		await authenticatedPage.getByTestId('input-name').fill('Header Test');
		await authenticatedPage.getByTestId('input-income').fill('3500');
		await authenticatedPage.getByTestId('input-fixed-costs').fill('1200');
		await authenticatedPage.getByTestId('input-desired-rate').fill('400');
		await authenticatedPage.getByTestId('select-employment-status').selectOption('self_employed');
		await authenticatedPage.getByTestId('radio-payment-default-no').check();
		await authenticatedPage.getByTestId('btn-save-draft').click();
		await authenticatedPage.waitForURL(/\/applications\/\d+/);

		await authenticatedPage.goto('/applications');

		const downloadPromise = authenticatedPage.waitForEvent('download');
		await authenticatedPage.getByTestId('applications-export-button').click();
		const download = await downloadPromise;

		const stream = await download.createReadStream();
		const chunks: Buffer[] = [];
		for await (const chunk of stream) {
			chunks.push(chunk as Buffer);
		}
		const content = Buffer.concat(chunks).toString('utf-8');

		const headerLine = content.replace(/^\uFEFF/, '').split('\r\n')[0];
		expect(headerLine).toBe('Name;Beschäftigungsstatus;Status;Score;Ampel;Gewünschte Rate;Erstellt am');
	});

	test('should export with status filter applied', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/applications/new');
		await authenticatedPage.getByTestId('input-name').fill('Filter Export Draft');
		await authenticatedPage.getByTestId('input-income').fill('4000');
		await authenticatedPage.getByTestId('input-fixed-costs').fill('1500');
		await authenticatedPage.getByTestId('input-desired-rate').fill('500');
		await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
		await authenticatedPage.getByTestId('radio-payment-default-no').check();
		await authenticatedPage.getByTestId('btn-save-draft').click();
		await authenticatedPage.waitForURL(/\/applications\/\d+/);

		await authenticatedPage.goto('/applications?status=draft');

		const downloadPromise = authenticatedPage.waitForEvent('download');
		await authenticatedPage.getByTestId('applications-export-button').click();
		const download = await downloadPromise;

		const stream = await download.createReadStream();
		const chunks: Buffer[] = [];
		for await (const chunk of stream) {
			chunks.push(chunk as Buffer);
		}
		const content = Buffer.concat(chunks).toString('utf-8');
		const lines = content.replace(/^\uFEFF/, '').split('\r\n').filter(Boolean);

		expect(lines.length).toBeGreaterThanOrEqual(2);
		for (const line of lines.slice(1)) {
			expect(line).toContain('Entwurf');
		}
	});

	test('should export all applications without filter', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/applications/new');
		await authenticatedPage.getByTestId('input-name').fill('No Filter Export');
		await authenticatedPage.getByTestId('input-income').fill('5000');
		await authenticatedPage.getByTestId('input-fixed-costs').fill('2000');
		await authenticatedPage.getByTestId('input-desired-rate').fill('600');
		await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
		await authenticatedPage.getByTestId('radio-payment-default-no').check();
		await authenticatedPage.getByTestId('btn-save-draft').click();
		await authenticatedPage.waitForURL(/\/applications\/\d+/);

		await authenticatedPage.goto('/applications');

		const downloadPromise = authenticatedPage.waitForEvent('download');
		await authenticatedPage.getByTestId('applications-export-button').click();
		const download = await downloadPromise;

		const stream = await download.createReadStream();
		const chunks: Buffer[] = [];
		for await (const chunk of stream) {
			chunks.push(chunk as Buffer);
		}
		const content = Buffer.concat(chunks).toString('utf-8');
		const lines = content.replace(/^\uFEFF/, '').split('\r\n').filter(Boolean);

		expect(lines.length).toBeGreaterThanOrEqual(2);
	});
});
