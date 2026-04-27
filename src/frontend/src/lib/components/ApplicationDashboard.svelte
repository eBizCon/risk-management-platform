<script lang="ts">
	import { Chart, registerables } from 'chart.js';
	import { Clock, FileText, CheckCircle, XCircle } from 'lucide-svelte';
	import type { DashboardStats } from '$lib/types';

	let { stats }: { stats: DashboardStats } = $props();

	Chart.register(...registerables);

	let barCanvas: HTMLCanvasElement | undefined = $state();
	let pieCanvas: HTMLCanvasElement | undefined = $state();
	let barChart: Chart | undefined;
	let pieChart: Chart | undefined;

	const statusColors = {
		draft: '#3b82f6',
		submitted: '#6b7280',
		approved: '#0e7c3a',
		rejected: '#b91c1c'
	};

	function renderCharts() {
		if (!barCanvas || !pieCanvas) return;

		barChart?.destroy();
		pieChart?.destroy();

		const labels = ['Entwurf', 'Eingereicht', 'Genehmigt', 'Abgelehnt'];
		const data = [stats.draft, stats.submitted, stats.approved, stats.rejected];
		const colors = [
			statusColors.draft,
			statusColors.submitted,
			statusColors.approved,
			statusColors.rejected
		];

		barChart = new Chart(barCanvas, {
			type: 'bar',
			data: {
				labels,
				datasets: [
					{
						data,
						backgroundColor: colors,
						borderWidth: 0,
						borderRadius: 4
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
						grid: {
							color: '#e5e7eb'
						}
					},
					x: {
						grid: {
							display: false
						}
					}
				}
			}
		});

		const pieLabels: string[] = [];
		const pieData: number[] = [];
		const pieColors: string[] = [];
		const total = stats.total;

		if (stats.draft > 0) {
			pieLabels.push(`Entwurf: ${Math.round((stats.draft / total) * 100)}%`);
			pieData.push(stats.draft);
			pieColors.push(statusColors.draft);
		}
		if (stats.submitted > 0) {
			pieLabels.push(`Eingereicht: ${Math.round((stats.submitted / total) * 100)}%`);
			pieData.push(stats.submitted);
			pieColors.push(statusColors.submitted);
		}
		if (stats.approved > 0) {
			pieLabels.push(`Genehmigt: ${Math.round((stats.approved / total) * 100)}%`);
			pieData.push(stats.approved);
			pieColors.push(statusColors.approved);
		}
		if (stats.rejected > 0) {
			pieLabels.push(`Abgelehnt: ${Math.round((stats.rejected / total) * 100)}%`);
			pieData.push(stats.rejected);
			pieColors.push(statusColors.rejected);
		}

		pieChart = new Chart(pieCanvas, {
			type: 'pie',
			data: {
				labels: pieLabels,
				datasets: [
					{
						data: pieData,
						backgroundColor: pieColors,
						borderWidth: 2,
						borderColor: '#ffffff'
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
							pointStyle: 'rect',
							padding: 16
						}
					}
				}
			}
		});
	}

	$effect(() => {
		if (barCanvas && pieCanvas && stats) {
			renderCharts();
		}
		return () => {
			barChart?.destroy();
			pieChart?.destroy();
		};
	});
</script>

<div class="space-y-6" data-testid="application-dashboard">
	<div>
		<h2 class="text-2xl sm:text-3xl font-bold text-primary" data-testid="dashboard-heading">
			Antrags-Dashboard
		</h2>
		<p class="text-secondary mt-1" data-testid="dashboard-total">
			Gesamt: {stats.total} Anträge
		</p>
	</div>

	<div
		class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4"
		data-testid="dashboard-status-cards"
	>
		<div
			class="card p-4 border-l-4 border-l-info"
			data-testid="dashboard-card-draft"
		>
			<div class="flex items-center justify-between">
				<div>
					<div class="text-sm text-secondary">Entwurf</div>
					<div class="text-3xl font-bold text-primary">{stats.draft}</div>
				</div>
				<div class="w-10 h-10 rounded-full flex items-center justify-center text-info bg-info/10">
					<Clock class="w-5 h-5" />
				</div>
			</div>
		</div>

		<div
			class="card p-4 border-l-4 border-l-text-muted"
			data-testid="dashboard-card-submitted"
		>
			<div class="flex items-center justify-between">
				<div>
					<div class="text-sm text-secondary">Eingereicht</div>
					<div class="text-3xl font-bold text-primary">{stats.submitted}</div>
				</div>
				<div
					class="w-10 h-10 rounded-full flex items-center justify-center text-text-muted bg-text-muted/10"
				>
					<FileText class="w-5 h-5" />
				</div>
			</div>
		</div>

		<div
			class="card p-4 border-l-4 border-l-success"
			data-testid="dashboard-card-approved"
		>
			<div class="flex items-center justify-between">
				<div>
					<div class="text-sm text-secondary">Genehmigt</div>
					<div class="text-3xl font-bold text-primary">{stats.approved}</div>
				</div>
				<div
					class="w-10 h-10 rounded-full flex items-center justify-center text-success bg-success/10"
				>
					<CheckCircle class="w-5 h-5" />
				</div>
			</div>
		</div>

		<div
			class="card p-4 border-l-4 border-l-danger"
			data-testid="dashboard-card-rejected"
		>
			<div class="flex items-center justify-between">
				<div>
					<div class="text-sm text-secondary">Abgelehnt</div>
					<div class="text-3xl font-bold text-primary">{stats.rejected}</div>
				</div>
				<div
					class="w-10 h-10 rounded-full flex items-center justify-center text-danger bg-danger/10"
				>
					<XCircle class="w-5 h-5" />
				</div>
			</div>
		</div>
	</div>

	<div class="grid grid-cols-1 md:grid-cols-2 gap-6">
		<div class="card p-6" data-testid="dashboard-bar-chart">
			<h3 class="text-lg font-bold text-primary mb-4">Antrag nach Status</h3>
			<div class="h-64">
				<canvas bind:this={barCanvas}></canvas>
			</div>
		</div>

		<div class="card p-6" data-testid="dashboard-pie-chart">
			<h3 class="text-lg font-bold text-primary mb-4">Verteilung</h3>
			<div class="h-64">
				<canvas bind:this={pieCanvas}></canvas>
			</div>
		</div>
	</div>
</div>
