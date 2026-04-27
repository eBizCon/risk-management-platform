<script lang="ts">
	import { onMount } from 'svelte';
	import { Chart, registerables } from 'chart.js';
	import { FileText, Send, CheckCircle, XCircle } from 'lucide-svelte';
	import type { DashboardStats } from '$lib/types';

	Chart.register(...registerables);

	interface Props {
		stats: DashboardStats;
	}

	let { stats }: Props = $props();

	const total = $derived(stats.draft + stats.submitted + stats.approved + stats.rejected);

	let barCanvas: HTMLCanvasElement;
	let pieCanvas: HTMLCanvasElement;
	let barChart: Chart | null = null;
	let pieChart: Chart | null = null;

	const chartColors = {
		draft: '#3B82F6',
		submitted: '#6B7280',
		approved: '#10B981',
		rejected: '#EF4444'
	};

	function createBarChart() {
		if (barChart) barChart.destroy();
		barChart = new Chart(barCanvas, {
			type: 'bar',
			data: {
				labels: ['Entwurf', 'Eingereicht', 'Genehmigt', 'Abgelehnt'],
				datasets: [
					{
						data: [stats.draft, stats.submitted, stats.approved, stats.rejected],
						backgroundColor: [
							chartColors.draft,
							chartColors.submitted,
							chartColors.approved,
							chartColors.rejected
						],
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
						ticks: {
							stepSize: 1,
							precision: 0
						},
						grid: { color: '#e5e7eb' }
					},
					x: {
						grid: { display: false }
					}
				}
			}
		});
	}

	function createPieChart() {
		if (pieChart) pieChart.destroy();
		const values = [stats.draft, stats.submitted, stats.approved, stats.rejected];
		const labels = ['Entwurf', 'Eingereicht', 'Genehmigt', 'Abgelehnt'];
		const colors = [
			chartColors.draft,
			chartColors.submitted,
			chartColors.approved,
			chartColors.rejected
		];

		pieChart = new Chart(pieCanvas, {
			type: 'pie',
			data: {
				labels,
				datasets: [
					{
						data: values,
						backgroundColor: colors,
						borderWidth: 0
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
							usePointStyle: true,
							padding: 16,
							generateLabels: (chart) => {
								const data = chart.data;
								if (!data.labels || !data.datasets.length) return [];
								const dataset = data.datasets[0];
								const sum = (dataset.data as number[]).reduce(
									(a: number, b: number) => a + b,
									0
								);
								return data.labels.map((label, i) => {
									const value = dataset.data[i] as number;
									const percent = sum > 0 ? Math.round((value / sum) * 100) : 0;
									const bgColors = dataset.backgroundColor as string[];
									return {
										text: value > 0 ? `${label}: ${percent}%` : (label as string),
										fillStyle: bgColors[i],
										strokeStyle: bgColors[i],
										hidden: false,
										index: i,
										pointStyle: 'circle'
									};
								});
							}
						}
					},
					tooltip: {
						callbacks: {
							label: (context) => {
								const value = context.parsed;
								const sum = (context.dataset.data as number[]).reduce(
									(a: number, b: number) => a + b,
									0
								);
								const percent = sum > 0 ? Math.round((value / sum) * 100) : 0;
								return `${context.label}: ${value} (${percent}%)`;
							}
						}
					}
				}
			}
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

<section class="space-y-6" data-testid="application-dashboard">
	<div>
		<h2 class="text-2xl sm:text-3xl font-bold text-primary" data-testid="dashboard-heading">
			Antrags-Dashboard
		</h2>
		<p class="text-secondary mt-1" data-testid="dashboard-total">
			Gesamt: {total} Anträge
		</p>
	</div>

	<div
		class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4"
		data-testid="dashboard-status-cards"
	>
		<div class="rounded-lg p-5 bg-blue-50" data-testid="dashboard-card-draft">
			<div class="flex items-center justify-between">
				<div>
					<p class="text-sm text-secondary">Entwurf</p>
					<p class="text-3xl font-bold text-primary mt-1" data-testid="dashboard-count-draft">
						{stats.draft}
					</p>
				</div>
				<div class="w-10 h-10 rounded-lg flex items-center justify-center bg-blue-100 text-blue-600">
					<FileText class="w-5 h-5" />
				</div>
			</div>
		</div>

		<div class="rounded-lg p-5 bg-gray-50" data-testid="dashboard-card-submitted">
			<div class="flex items-center justify-between">
				<div>
					<p class="text-sm text-secondary">Eingereicht</p>
					<p
						class="text-3xl font-bold text-primary mt-1"
						data-testid="dashboard-count-submitted"
					>
						{stats.submitted}
					</p>
				</div>
				<div class="w-10 h-10 rounded-lg flex items-center justify-center bg-gray-200 text-gray-600">
					<Send class="w-5 h-5" />
				</div>
			</div>
		</div>

		<div class="rounded-lg p-5 bg-green-50" data-testid="dashboard-card-approved">
			<div class="flex items-center justify-between">
				<div>
					<p class="text-sm text-secondary">Genehmigt</p>
					<p
						class="text-3xl font-bold text-primary mt-1"
						data-testid="dashboard-count-approved"
					>
						{stats.approved}
					</p>
				</div>
				<div
					class="w-10 h-10 rounded-lg flex items-center justify-center bg-green-100 text-green-600"
				>
					<CheckCircle class="w-5 h-5" />
				</div>
			</div>
		</div>

		<div class="rounded-lg p-5 bg-red-50" data-testid="dashboard-card-rejected">
			<div class="flex items-center justify-between">
				<div>
					<p class="text-sm text-secondary">Abgelehnt</p>
					<p
						class="text-3xl font-bold text-primary mt-1"
						data-testid="dashboard-count-rejected"
					>
						{stats.rejected}
					</p>
				</div>
				<div class="w-10 h-10 rounded-lg flex items-center justify-center bg-red-100 text-red-600">
					<XCircle class="w-5 h-5" />
				</div>
			</div>
		</div>
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
</section>
