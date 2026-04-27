import { test } from './fixtures';
import { test as base, expect as baseExpect } from '@playwright/test';
import { clearTestSessions } from './helpers/auth';

test.describe('Home Page - Applicant View', () => {
	test.use({ userRole: 'applicant' });

	test('should display home structure and feature cards', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await baseExpect(authenticatedPage.getByTestId('home-page')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('home-hero')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('home-features')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('home-feature-automation')).toBeVisible();
	});

	test('should show applicant role actions', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await baseExpect(authenticatedPage.getByTestId('home-role-section')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('home-applicant-section')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('home-applicant-new-application-link')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('home-applicant-applications-link')).toBeVisible();
	});

	test('should show dashboard for applicant', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await baseExpect(authenticatedPage.getByTestId('dashboard')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-header')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-total')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-cards')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-card-entwurf')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-card-eingereicht')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-card-genehmigt')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-card-abgelehnt')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-charts')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-bar-chart')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-pie-chart')).toBeVisible();
	});

	test('should display consistent total in dashboard header', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		const totalText = await authenticatedPage.getByTestId('dashboard-total').textContent();
		const match = totalText?.match(/Gesamt:\s*(\d+)\s*Anträge/);
		baseExpect(match).not.toBeNull();

		const total = parseInt(match![1], 10);
		let sum = 0;
		for (const status of ['entwurf', 'eingereicht', 'genehmigt', 'abgelehnt']) {
			const countText = await authenticatedPage
				.getByTestId(`dashboard-count-${status}`)
				.textContent();
			sum += parseInt(countText ?? '0', 10);
		}
		baseExpect(sum).toBe(total);
	});
});

test.describe('Home Page - Processor View', () => {
	test.use({ userRole: 'processor' });

	test('should show processor role action', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await baseExpect(authenticatedPage.getByTestId('home-role-section')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('home-processor-section')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('home-processor-worklist-link')).toBeVisible();
	});

	test('should show dashboard for processor', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await baseExpect(authenticatedPage.getByTestId('dashboard')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-header')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-total')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-cards')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-charts')).toBeVisible();
	});
});

base.describe('Home Page - Unauthenticated User', () => {
	base('should show guest section and login action', async ({ page }) => {
		await clearTestSessions(page);
		await page.goto('/');
		await baseExpect(page.getByTestId('home-role-section')).toBeVisible();
		await baseExpect(page.getByTestId('home-guest-section')).toBeVisible();
		await baseExpect(page.getByTestId('hero-login')).toBeVisible();
	});

	base('should not show dashboard for unauthenticated user', async ({ page }) => {
		await clearTestSessions(page);
		await page.goto('/');
		await baseExpect(page.getByTestId('dashboard')).not.toBeVisible();
	});
});
