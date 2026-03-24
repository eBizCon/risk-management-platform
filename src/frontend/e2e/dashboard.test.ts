import { test } from './fixtures';
import { test as base, expect as baseExpect } from '@playwright/test';
import { createTestSession, clearTestSessions } from './helpers/auth';

test.describe('Dashboard - Applicant View', () => {
	test.use({ userRole: 'applicant' });

	test('should display dashboard section with stats and charts', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await baseExpect(authenticatedPage.getByTestId('dashboard-section')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-stats')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-charts')).toBeVisible();
	});

	test('should display all four stat cards', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await baseExpect(authenticatedPage.getByTestId('stat-card-draft')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('stat-card-submitted')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('stat-card-approved')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('stat-card-rejected')).toBeVisible();
	});

	test('should display bar chart and pie chart', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await baseExpect(authenticatedPage.getByTestId('dashboard-bar-chart')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-pie-chart')).toBeVisible();
		await baseExpect(authenticatedPage.getByText('Antrag nach Status')).toBeVisible();
		await baseExpect(authenticatedPage.getByText('Verteilung')).toBeVisible();
	});

	test('should show stat card values as numbers', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		const draftValue = authenticatedPage.getByTestId('stat-card-draft-value');
		await baseExpect(draftValue).toBeVisible();
		const text = await draftValue.textContent();
		baseExpect(text?.trim()).toMatch(/^\d+$/);
	});
});

test.describe('Dashboard - Processor View', () => {
	test.use({ userRole: 'processor' });

	test('should display dashboard section with stats and charts', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await baseExpect(authenticatedPage.getByTestId('dashboard-section')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-stats')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-charts')).toBeVisible();
	});

	test('should display all four stat cards', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await baseExpect(authenticatedPage.getByTestId('stat-card-draft')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('stat-card-submitted')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('stat-card-approved')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('stat-card-rejected')).toBeVisible();
	});
});

base.describe('Dashboard - Empty State', () => {
	base('should show zero values when user has no applications', async ({ page }) => {
		await createTestSession(page, 'applicant', { email: 'empty-dashboard@example.com' });
		await page.goto('/');
		await baseExpect(page.getByTestId('dashboard-section')).toBeVisible();

		await baseExpect(page.getByTestId('stat-card-draft-value')).toHaveText('0');
		await baseExpect(page.getByTestId('stat-card-submitted-value')).toHaveText('0');
		await baseExpect(page.getByTestId('stat-card-approved-value')).toHaveText('0');
		await baseExpect(page.getByTestId('stat-card-rejected-value')).toHaveText('0');

		await clearTestSessions(page);
	});
});

base.describe('Dashboard - Unauthenticated User', () => {
	base('should not display dashboard section', async ({ page }) => {
		await page.goto('/');
		await baseExpect(page.getByTestId('dashboard-section')).not.toBeVisible();
	});
});
