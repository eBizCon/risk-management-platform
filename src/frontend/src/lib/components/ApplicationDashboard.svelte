<script lang="ts">
	import { Chart, BarController, BarElement, CategoryScale, LinearScale, ArcElement, PieController, Tooltip, Legend } from 'chart.js';
	import type { DashboardStats } from '$lib/types';
	import { FileText, Send, CheckCircle, XCircle } from 'lucide-svelte';

	Chart.register(BarController, BarElement, CategoryScale, LinearScale, ArcElement, PieController, Tooltip, Legend);

	let { stats }: { stats: DashboardStats } = $props();

	const total = $derived(stats.draft + stats.submitted + stats.approved + stats.rejected);

	const cards = $derived([
		{ label: 'Entwurf', count: stats.draft, icon: FileText, colorClass: 'status-draft', testId: 'dashboard-card-draft' },
		{ label: 'Eingereicht', count: stats.submitted, icon: Send, colorClass: 'status-submitted', testId: 'dashboard-card-submitted' },
		{ label: 'Genehmigt', count: stats.approved, icon: CheckCircle, colorClass: 'status-approved', testId: 'dashboard-card-approved' },
		{ label: 'Abgelehnt', count: stats.rejected, icon: XCircle, colorClass: 'status-rejected', testId: 'dashboard-card-rejected' }
	]);

	const chartColors = {
		draft: '#6b7280',
		submitted: '#1d4ed8',
		approved: '#0e7c3a',
		rejected: '#b91c1c'
	};

	let barCanvas: HTMLCanvasElement;
	let pieCanvas: HTMLCanvasElement;
	let barChart: Chart | null = null;
	let pieChart: Chart | null = null;

	$effect(() => {
		const data = [stats.draft, stats.submitted, stats.approved, stats.rejected];
		const labels = ['Entwurf', 'Eingereicht', 'Genehmigt', 'Abgelehnt'];
		const colors = [chartColors.draft, chartColors.submitted, chartColors.approved, chartColors.rejected];

		if (barChart) barChart.destroy();
		if (barCanvas) {
			barChart = new Chart(barCanvas, {
				type: 'bar',
				data: {
					labels,
					datasets: [{
						data,
						backgroundColor: colors,
						borderRadius: 4
					}]
				},
				options: {
					responsive: true,
					maintainAspectRatio: false,
					plugins: {
						legend: { display: false },
						tooltip: { enabled: true }
					},
					scales: {
						y: {
							beginAtZero: true,
							ticks: { stepSize: 1, precision: 0 }
						}
					}
				}
			});
		}

		if (pieChart) pieChart.destroy();
		if (pieCanvas) {
			const currentTotal = stats.draft + stats.submitted + stats.approved + stats.rejected;
			pieChart = new Chart(pieCanvas, {
				type: 'pie',
				data: {
					labels,
					datasets: [{
						data,
						backgroundColor: colors
					}]
				},
				options: {
					responsive: true,
					maintainAspectRatio: false,
					plugins: {
						legend: { position: 'bottom' },
						tooltip: {
							callbacks: {
								label: (context) => {
									const value = context.parsed;
									if (currentTotal === 0 || value === 0) return context.label ?? '';
									const pct = Math.round((value / currentTotal) * 100);
									return `${context.label}: ${pct}%`;
								}
							}
						}
					}
				}
			});
		}

		return () => {
			if (barChart) barChart.destroy();
			if (pieChart) pieChart.destroy();
		};
	});
</script>

<section class="space-y-6" data-testid="dashboard-section">
	<div>
		<h2 class="text-2xl font-bold text-primary" data-testid="dashboard-heading">Antrags-Dashboard</h2>
		<p class="text-secondary mt-1" data-testid="dashboard-total">Gesamt: {total} Anträge</p>
	</div>

	<div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
		{#each cards as card}
			<div class="card p-4 flex items-center gap-4" data-testid={card.testId}>
				<div class="w-10 h-10 rounded-lg flex items-center justify-center {card.colorClass}">
					<card.icon class="w-5 h-5" />
				</div>
				<div>
					<p class="text-sm text-secondary">{card.label}</p>
					<p class="text-2xl font-bold text-primary">{card.count}</p>
				</div>
			</div>
		{/each}
	</div>

	<div class="grid grid-cols-1 md:grid-cols-2 gap-6">
		<div class="card p-4" data-testid="dashboard-bar-chart">
			<h3 class="text-lg font-semibold text-primary mb-4">Anträge nach Status</h3>
			<div class="h-64">
				<canvas bind:this={barCanvas}></canvas>
			</div>
		</div>
		<div class="card p-4" data-testid="dashboard-pie-chart">
			<h3 class="text-lg font-semibold text-primary mb-4">Verteilung</h3>
			<div class="h-64">
				<canvas bind:this={pieCanvas}></canvas>
			</div>
		</div>
	</div>
</section>
