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
		await baseExpect(
			authenticatedPage.getByTestId('home-applicant-new-application-link')
		).toBeVisible();
		await baseExpect(
			authenticatedPage.getByTestId('home-applicant-applications-link')
		).toBeVisible();
	});

	test('should display dashboard stats for applicant', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await baseExpect(authenticatedPage.getByTestId('dashboard-stats')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-header')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-total')).toContainText('Gesamt:');
		await baseExpect(authenticatedPage.getByTestId('dashboard-total')).toContainText('Anträge');
	});

	test('should display all four status cards', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await baseExpect(authenticatedPage.getByTestId('dashboard-card-draft')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-card-submitted')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-card-approved')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-card-rejected')).toBeVisible();
	});

	test('should display dashboard charts', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await baseExpect(authenticatedPage.getByTestId('dashboard-bar-chart')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-pie-chart')).toBeVisible();
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

	test('should display dashboard stats for processor', async ({ authenticatedPage }) => {
		await authenticatedPage.goto('/');
		await baseExpect(authenticatedPage.getByTestId('dashboard-stats')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-header')).toBeVisible();
		await baseExpect(authenticatedPage.getByTestId('dashboard-total')).toContainText('Gesamt:');
		await baseExpect(authenticatedPage.getByTestId('dashboard-status-cards')).toBeVisible();
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

	base('should not show dashboard stats for unauthenticated user', async ({ page }) => {
		await clearTestSessions(page);
		await page.goto('/');
		await baseExpect(page.getByTestId('dashboard-stats')).not.toBeVisible();
	});
});
