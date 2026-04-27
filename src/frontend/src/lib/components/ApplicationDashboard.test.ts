/// <reference types="@testing-library/jest-dom" />
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/svelte';
import ApplicationDashboard from './ApplicationDashboard.svelte';
import type { DashboardStats } from '$lib/types';

vi.mock('chart.js', () => {
	class MockChart {
		static register = vi.fn();
		destroy = vi.fn();
	}
	return { Chart: MockChart, registerables: [] };
});

const defaultStats: DashboardStats = {
	total: 12,
	draft: 4,
	submitted: 3,
	approved: 3,
	rejected: 2
};

describe('ApplicationDashboard', () => {
	it('should render heading and total count', () => {
		render(ApplicationDashboard, { props: { stats: defaultStats } });

		expect(screen.getByTestId('dashboard-heading')).toHaveTextContent('Antrags-Dashboard');
		expect(screen.getByTestId('dashboard-total')).toHaveTextContent('Gesamt: 12 Anträge');
	});

	it('should render all four status cards with correct counts', () => {
		render(ApplicationDashboard, { props: { stats: defaultStats } });

		expect(screen.getByTestId('dashboard-count-draft')).toHaveTextContent('4');
		expect(screen.getByTestId('dashboard-count-submitted')).toHaveTextContent('3');
		expect(screen.getByTestId('dashboard-count-approved')).toHaveTextContent('3');
		expect(screen.getByTestId('dashboard-count-rejected')).toHaveTextContent('2');
	});

	it('should render chart containers', () => {
		render(ApplicationDashboard, { props: { stats: defaultStats } });

		expect(screen.getByTestId('dashboard-bar-chart')).toBeInTheDocument();
		expect(screen.getByTestId('dashboard-pie-chart')).toBeInTheDocument();
	});

	it('should display zero counts correctly', () => {
		const zeroStats: DashboardStats = {
			total: 0,
			draft: 0,
			submitted: 0,
			approved: 0,
			rejected: 0
		};
		render(ApplicationDashboard, { props: { stats: zeroStats } });

		expect(screen.getByTestId('dashboard-total')).toHaveTextContent('Gesamt: 0 Anträge');
		expect(screen.getByTestId('dashboard-count-draft')).toHaveTextContent('0');
		expect(screen.getByTestId('dashboard-count-submitted')).toHaveTextContent('0');
		expect(screen.getByTestId('dashboard-count-approved')).toHaveTextContent('0');
		expect(screen.getByTestId('dashboard-count-rejected')).toHaveTextContent('0');
	});

	it('should compute total from individual status counts', () => {
		const stats: DashboardStats = {
			total: 10,
			draft: 5,
			submitted: 2,
			approved: 2,
			rejected: 1
		};
		render(ApplicationDashboard, { props: { stats } });

		expect(screen.getByTestId('dashboard-total')).toHaveTextContent('Gesamt: 10 Anträge');
	});
});
