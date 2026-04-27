/// <reference types="@testing-library/jest-dom" />
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/svelte';
import ApplicationDashboard from './ApplicationDashboard.svelte';
import type { DashboardStats } from '$lib/types';

vi.mock('chart.js', () => {
	class MockChart {
		destroy = vi.fn();
		static register = vi.fn();
		constructor() {}
	}
	return {
		Chart: MockChart,
		registerables: []
	};
});

const sampleStats: DashboardStats = {
	draft: 4,
	submitted: 3,
	approved: 3,
	rejected: 2,
	total: 12
};

describe('ApplicationDashboard', () => {
	it('should render heading and total', () => {
		render(ApplicationDashboard, { props: { stats: sampleStats } });

		expect(screen.getByTestId('dashboard-heading')).toHaveTextContent('Antrags-Dashboard');
		expect(screen.getByTestId('dashboard-total')).toHaveTextContent('Gesamt: 12 Anträge');
	});

	it('should render four status cards with correct counts', () => {
		render(ApplicationDashboard, { props: { stats: sampleStats } });

		expect(screen.getByTestId('dashboard-card-draft')).toHaveTextContent('4');
		expect(screen.getByTestId('dashboard-card-draft')).toHaveTextContent('Entwurf');

		expect(screen.getByTestId('dashboard-card-submitted')).toHaveTextContent('3');
		expect(screen.getByTestId('dashboard-card-submitted')).toHaveTextContent('Eingereicht');

		expect(screen.getByTestId('dashboard-card-approved')).toHaveTextContent('3');
		expect(screen.getByTestId('dashboard-card-approved')).toHaveTextContent('Genehmigt');

		expect(screen.getByTestId('dashboard-card-rejected')).toHaveTextContent('2');
		expect(screen.getByTestId('dashboard-card-rejected')).toHaveTextContent('Abgelehnt');
	});

	it('should render chart containers', () => {
		render(ApplicationDashboard, { props: { stats: sampleStats } });

		expect(screen.getByTestId('dashboard-bar-chart')).toBeInTheDocument();
		expect(screen.getByTestId('dashboard-pie-chart')).toBeInTheDocument();
		expect(screen.getByTestId('dashboard-bar-chart')).toHaveTextContent('Antrag nach Status');
		expect(screen.getByTestId('dashboard-pie-chart')).toHaveTextContent('Verteilung');
	});

	it('should render with zero stats', () => {
		const zeroStats: DashboardStats = {
			draft: 0,
			submitted: 0,
			approved: 0,
			rejected: 0,
			total: 0
		};
		render(ApplicationDashboard, { props: { stats: zeroStats } });

		expect(screen.getByTestId('dashboard-total')).toHaveTextContent('Gesamt: 0 Anträge');
		expect(screen.getByTestId('dashboard-card-draft')).toHaveTextContent('0');
		expect(screen.getByTestId('dashboard-card-submitted')).toHaveTextContent('0');
		expect(screen.getByTestId('dashboard-card-approved')).toHaveTextContent('0');
		expect(screen.getByTestId('dashboard-card-rejected')).toHaveTextContent('0');
	});

	it('should have data-testid on dashboard wrapper', () => {
		render(ApplicationDashboard, { props: { stats: sampleStats } });
		expect(screen.getByTestId('application-dashboard')).toBeInTheDocument();
	});
});
