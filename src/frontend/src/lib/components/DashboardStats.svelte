<script lang="ts">
	import type { DashboardStats } from '$lib/types';
	import { onMount } from 'svelte';
	import { FileEdit, Send, CheckCircle, XCircle } from 'lucide-svelte';
	import Chart from 'chart.js/auto';

	interface Props {
		stats: DashboardStats;
	}

	let { stats }: Props = $props();

	let barCanvas: HTMLCanvasElement;
	let pieCanvas: HTMLCanvasElement;
	let barChart: Chart | null = null;
	let pieChart: Chart | null = null;

	const statusItems = $derived([
		{
			key: 'draft' as const,
			label: 'Entwurf',
			value: stats.draft,
			icon: FileEdit,
			bgClass: 'bg-blue-50',
			textClass: 'text-blue-600'
		},
		{
			key: 'submitted' as const,
			label: 'Eingereicht',
			value: stats.submitted,
			icon: Send,
			bgClass: 'bg-gray-50',
			textClass: 'text-gray-600'
		},
		{
			key: 'approved' as const,
			label: 'Genehmigt',
			value: stats.approved,
			icon: CheckCircle,
			bgClass: 'bg-green-50',
			textClass: 'text-green-600'
		},
		{
			key: 'rejected' as const,
			label: 'Abgelehnt',
			value: stats.rejected,
			icon: XCircle,
			bgClass: 'bg-red-50',
			textClass: 'text-red-600'
		}
	]);

	const chartColors = ['#3b82f6', '#6b7280', '#22c55e', '#ef4444'];
	const chartLabels = ['Entwurf', 'Eingereicht', 'Genehmigt', 'Abgelehnt'];

	function createBarChart() {
		if (barChart) barChart.destroy();
		barChart = new Chart(barCanvas, {
			type: 'bar',
			data: {
				labels: chartLabels,
				datasets: [
					{
						data: [stats.draft, stats.submitted, stats.approved, stats.rejected],
						backgroundColor: chartColors,
						borderRadius: 4,
						maxBarThickness: 48
					}
				]
			},
			options: {
				responsive: true,
				maintainAspectRatio: false,
				plugins: {
					legend: { display: false }
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

	function createPieChart() {
		if (pieChart) pieChart.destroy();
		const values = [stats.draft, stats.submitted, stats.approved, stats.rejected];

		pieChart = new Chart(pieCanvas, {
			type: 'pie',
			data: {
				labels: chartLabels,
				datasets: [
					{
						data: values,
						backgroundColor: chartColors
					}
				]
			},
			options: {
				responsive: true,
				maintainAspectRatio: false,
				plugins: {
					legend: {
						position: 'bottom',
						labels: {
							padding: 16,
							usePointStyle: true,
							pointStyle: 'circle'
						}
					},
					tooltip: {
						callbacks: {
							label: (context) => {
								const value = context.parsed;
								const total = stats.total;
								if (total === 0) return `${context.label}: 0`;
								const pct = Math.round((value / total) * 100);
								return `${context.label}: ${pct}%`;
							}
						}
					}
				}
			},
			plugins: [
				{
					id: 'pieLabels',
					afterDraw(chart) {
						const total = stats.total;
						if (total === 0) return;

						const { ctx } = chart;
						const meta = chart.getDatasetMeta(0);

						meta.data.forEach((element, index) => {
							const value = values[index];
							if (value === 0) return;

							const pct = Math.round((value / total) * 100);
							const pos = element.tooltipPosition(false);
							if (pos.x == null || pos.y == null) return;

							ctx.save();
							ctx.fillStyle = '#ffffff';
							ctx.font = 'bold 12px sans-serif';
							ctx.textAlign = 'center';
							ctx.textBaseline = 'middle';
							ctx.fillText(`${pct}%`, pos.x, pos.y);
							ctx.restore();
						});
					}
				}
			]
		});
	}

	onMount(() => {
		createBarChart();
		createPieChart();

		return () => {
			barChart?.destroy();
			pieChart?.destroy();
		};
	});
</script>

<div class="space-y-6" data-testid="dashboard-stats">
	<div data-testid="dashboard-header">
		<h2 class="text-2xl sm:text-3xl font-bold text-primary">Antrags-Dashboard</h2>
		<p class="text-secondary mt-1" data-testid="dashboard-total">
			Gesamt: {stats.total} Anträge
		</p>
	</div>

	<div class="grid grid-cols-2 lg:grid-cols-4 gap-4" data-testid="dashboard-status-cards">
		{#each statusItems as item (item.key)}
			<div class="rounded-lg p-4 {item.bgClass}" data-testid="dashboard-card-{item.key}">
				<div class="flex items-center justify-between">
					<div>
						<p class="text-sm text-secondary">{item.label}</p>
						<p
							class="text-2xl sm:text-3xl font-bold {item.textClass}"
							data-testid="dashboard-count-{item.key}"
						>
							{item.value}
						</p>
					</div>
					<div class="{item.textClass} opacity-60">
						<item.icon class="w-8 h-8" />
					</div>
				</div>
			</div>
		{/each}
	</div>

	<div class="grid grid-cols-1 lg:grid-cols-2 gap-6" data-testid="dashboard-charts">
		<div class="card p-6" data-testid="dashboard-bar-chart">
			<h3 class="text-lg font-semibold text-primary mb-4">Antrag nach Status</h3>
			<div class="h-64">
				<canvas bind:this={barCanvas}></canvas>
			</div>
		</div>
		<div class="card p-6" data-testid="dashboard-pie-chart">
			<h3 class="text-lg font-semibold text-primary mb-4">Verteilung</h3>
			<div class="h-64">
				<canvas bind:this={pieCanvas}></canvas>
			</div>
		</div>
	</div>
</div>
