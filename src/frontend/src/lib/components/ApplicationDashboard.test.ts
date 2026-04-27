/// <reference types="@testing-library/jest-dom" />
import { render, screen } from '@testing-library/svelte';
import { describe, it, expect, vi } from 'vitest';
import ApplicationDashboard from './ApplicationDashboard.svelte';
import type { DashboardStats } from '$lib/types';

vi.mock('chart.js', () => {
	class MockChart {
		static register = vi.fn();
		destroy = vi.fn();
		constructor() {}
	}
	return {
		Chart: MockChart,
		BarController: class {},
		BarElement: class {},
		CategoryScale: class {},
		LinearScale: class {},
		ArcElement: class {},
		PieController: class {},
		Tooltip: class {},
		Legend: class {}
	};
});

const sampleStats: DashboardStats = {
	total: 12,
	draft: 4,
	submitted: 3,
	approved: 3,
	rejected: 2
};

describe('ApplicationDashboard', () => {
	it('should render the dashboard header with total count', () => {
		render(ApplicationDashboard, { props: { stats: sampleStats } });

		expect(screen.getByTestId('dashboard')).toBeInTheDocument();
		expect(screen.getByTestId('dashboard-header')).toBeInTheDocument();
		expect(screen.getByTestId('dashboard-total')).toHaveTextContent('Gesamt: 12 Anträge');
	});

	it('should render four status cards with correct counts', () => {
		render(ApplicationDashboard, { props: { stats: sampleStats } });

		expect(screen.getByTestId('dashboard-cards')).toBeInTheDocument();
		expect(screen.getByTestId('dashboard-count-entwurf')).toHaveTextContent('4');
		expect(screen.getByTestId('dashboard-count-eingereicht')).toHaveTextContent('3');
		expect(screen.getByTestId('dashboard-count-genehmigt')).toHaveTextContent('3');
		expect(screen.getByTestId('dashboard-count-abgelehnt')).toHaveTextContent('2');
	});

	it('should render chart containers', () => {
		render(ApplicationDashboard, { props: { stats: sampleStats } });

		expect(screen.getByTestId('dashboard-charts')).toBeInTheDocument();
		expect(screen.getByTestId('dashboard-bar-chart')).toBeInTheDocument();
		expect(screen.getByTestId('dashboard-pie-chart')).toBeInTheDocument();
	});

	it('should render zero counts correctly', () => {
		const zeroStats: DashboardStats = {
			total: 0,
			draft: 0,
			submitted: 0,
			approved: 0,
			rejected: 0
		};
		render(ApplicationDashboard, { props: { stats: zeroStats } });

		expect(screen.getByTestId('dashboard-total')).toHaveTextContent('Gesamt: 0 Anträge');
		expect(screen.getByTestId('dashboard-count-entwurf')).toHaveTextContent('0');
		expect(screen.getByTestId('dashboard-count-eingereicht')).toHaveTextContent('0');
		expect(screen.getByTestId('dashboard-count-genehmigt')).toHaveTextContent('0');
		expect(screen.getByTestId('dashboard-count-abgelehnt')).toHaveTextContent('0');
	});
});
