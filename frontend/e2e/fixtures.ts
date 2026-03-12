import { test as base, expect, type BrowserContext, type Page } from '@playwright/test';
import { clearTestSessions, createTestSession, type TestUserRole } from './helpers/auth';

type TestFixtures = {
	authenticatedPage: Page;
	authenticatedContext: BrowserContext;
	userRole: TestUserRole;
};

const test = base.extend<TestFixtures>({
	userRole: ['applicant', { option: true }],
	authenticatedPage: async ({ browser, userRole }, use) => {
		const context = await browser.newContext();
		const page = await context.newPage();
		await createTestSession(page, userRole);

		try {
			await use(page);
		} finally {
			await clearTestSessions(page);
			await context.close();
		}
	},
	authenticatedContext: async ({ browser, userRole }, use) => {
		const context = await browser.newContext();
		const page = await context.newPage();
		await createTestSession(page, userRole);

		try {
			await use(context);
		} finally {
			await clearTestSessions(page);
			await context.close();
		}
	}
});

export { test, expect };
