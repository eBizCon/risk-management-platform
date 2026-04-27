/// <reference types="@testing-library/jest-dom" />
import { render, screen } from '@testing-library/svelte';
import { describe, it, expect } from 'vitest';
import Dashboard from './Dashboard.svelte';
import type { DashboardStats } from '$lib/types';

describe('Dashboard', () => {
	const sampleStats: DashboardStats = {
		total: 12,
		draft: 4,
		submitted: 3,
		approved: 3,
		rejected: 2
	};

	it('should render the heading', () => {
		render(Dashboard, { props: { stats: sampleStats } });
		expect(screen.getByTestId('dashboard-heading')).toHaveTextContent('Antrags-Dashboard');
	});

	it('should display the total count', () => {
		render(Dashboard, { props: { stats: sampleStats } });
		expect(screen.getByTestId('dashboard-total')).toHaveTextContent('Gesamt: 12 Anträge');
	});

	it('should display all four status cards', () => {
		render(Dashboard, { props: { stats: sampleStats } });
		expect(screen.getByTestId('dashboard-card-draft')).toBeInTheDocument();
		expect(screen.getByTestId('dashboard-card-submitted')).toBeInTheDocument();
		expect(screen.getByTestId('dashboard-card-approved')).toBeInTheDocument();
		expect(screen.getByTestId('dashboard-card-rejected')).toBeInTheDocument();
	});

	it('should display correct counts in status cards', () => {
		render(Dashboard, { props: { stats: sampleStats } });
		expect(screen.getByTestId('dashboard-count-draft')).toHaveTextContent('4');
		expect(screen.getByTestId('dashboard-count-submitted')).toHaveTextContent('3');
		expect(screen.getByTestId('dashboard-count-approved')).toHaveTextContent('3');
		expect(screen.getByTestId('dashboard-count-rejected')).toHaveTextContent('2');
	});

	it('should render bar chart and pie chart sections', () => {
		render(Dashboard, { props: { stats: sampleStats } });
		expect(screen.getByTestId('dashboard-bar-chart')).toBeInTheDocument();
		expect(screen.getByTestId('dashboard-pie-chart')).toBeInTheDocument();
	});

	it('should display pie chart legend entries for non-zero values', () => {
		render(Dashboard, { props: { stats: sampleStats } });
		const legend = screen.getByTestId('dashboard-pie-legend');
		expect(legend).toHaveTextContent('Entwurf: 33%');
		expect(legend).toHaveTextContent('Eingereicht: 25%');
		expect(legend).toHaveTextContent('Genehmigt: 25%');
		expect(legend).toHaveTextContent('Abgelehnt: 17%');
	});

	it('should not display legend entries for zero values', () => {
		const statsWithZero: DashboardStats = {
			total: 5,
			draft: 3,
			submitted: 2,
			approved: 0,
			rejected: 0
		};
		render(Dashboard, { props: { stats: statsWithZero } });
		const legend = screen.getByTestId('dashboard-pie-legend');
		expect(legend).toHaveTextContent('Entwurf: 60%');
		expect(legend).toHaveTextContent('Eingereicht: 40%');
		expect(legend).not.toHaveTextContent('Genehmigt');
		expect(legend).not.toHaveTextContent('Abgelehnt');
	});

	it('should handle all-zero stats correctly', () => {
		const zeroStats: DashboardStats = {
			total: 0,
			draft: 0,
			submitted: 0,
			approved: 0,
			rejected: 0
		};
		render(Dashboard, { props: { stats: zeroStats } });
		expect(screen.getByTestId('dashboard-total')).toHaveTextContent('Gesamt: 0 Anträge');
		expect(screen.getByTestId('dashboard-count-draft')).toHaveTextContent('0');
	});

	it('should have consistent total equal to sum of all statuses', () => {
		render(Dashboard, { props: { stats: sampleStats } });
		const total = sampleStats.draft + sampleStats.submitted + sampleStats.approved + sampleStats.rejected;
		expect(total).toBe(sampleStats.total);
		expect(screen.getByTestId('dashboard-total')).toHaveTextContent(`Gesamt: ${total} Anträge`);
	});
});
