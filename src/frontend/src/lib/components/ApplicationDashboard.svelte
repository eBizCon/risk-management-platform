<script lang="ts">
	import { onMount } from 'svelte';
	import { FileText, Send, CheckCircle, XCircle } from 'lucide-svelte';
	import type { DashboardStats } from '$lib/types';
	import {
		Chart,
		BarController,
		BarElement,
		CategoryScale,
		LinearScale,
		ArcElement,
		PieController,
		Tooltip,
		Legend
	} from 'chart.js';

	Chart.register(
		BarController,
		BarElement,
		CategoryScale,
		LinearScale,
		ArcElement,
		PieController,
		Tooltip,
		Legend
	);

	let { stats }: { stats: DashboardStats } = $props();

	const cards = $derived([
		{ label: 'Entwurf', count: stats.draft, icon: FileText, colorClass: 'text-text-muted', bgClass: 'bg-bg-muted' },
		{ label: 'Eingereicht', count: stats.submitted, icon: Send, colorClass: 'text-info', bgClass: 'bg-info/15' },
		{
			label: 'Genehmigt',
			count: stats.approved,
			icon: CheckCircle,
			colorClass: 'text-success',
			bgClass: 'bg-success/15'
		},
		{
			label: 'Abgelehnt',
			count: stats.rejected,
			icon: XCircle,
			colorClass: 'text-danger',
			bgClass: 'bg-danger/15'
		}
	]);

	const statusColors = {
		draft: '#9ca3af',
		submitted: '#1d4ed8',
		approved: '#0e7c3a',
		rejected: '#b91c1c'
	};

	let barCanvas: HTMLCanvasElement;
	let pieCanvas: HTMLCanvasElement;
	let barChart: Chart | null = null;
	let pieChart: Chart | null = null;

	function createCharts() {
		const labels = ['Entwurf', 'Eingereicht', 'Genehmigt', 'Abgelehnt'];
		const data = [stats.draft, stats.submitted, stats.approved, stats.rejected];
		const colors = [statusColors.draft, statusColors.submitted, statusColors.approved, statusColors.rejected];

		barChart?.destroy();
		pieChart?.destroy();

		barChart = new Chart(barCanvas, {
			type: 'bar',
			data: {
				labels,
				datasets: [
					{
						label: 'Anzahl',
						data,
						backgroundColor: colors,
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
							callback: (value) => (Number.isInteger(value) ? value : null)
						}
					}
				}
			}
		});

		const displayedSum = data.reduce((a, b) => a + b, 0);
		pieChart = new Chart(pieCanvas, {
			type: 'pie',
			data: {
				labels: displayedSum > 0
					? labels.map((l, i) => {
						if (data[i] === 0) return l;
						const pct = Math.round((data[i] / displayedSum) * 100);
						return `${l}: ${pct}%`;
					  })
					: labels,
				datasets: [
					{
						data,
						backgroundColor: colors
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
							filter: (item, chartData) => {
								const idx = item.index ?? 0;
								const val = chartData.datasets[0].data[idx] as number;
								return val > 0;
							}
						}
					},
					tooltip: {
						callbacks: {
							label: (ctx) => {
								const value = ctx.raw as number;
								if (displayedSum === 0) return `${ctx.label}: 0`;
								const pct = Math.round((value / displayedSum) * 100);
								return `${value} (${pct}%)`;
							}
						}
					}
				}
			}
		});
	}

	let mounted = false;

	onMount(() => {
		mounted = true;
		createCharts();
		return () => {
			mounted = false;
			barChart?.destroy();
			pieChart?.destroy();
		};
	});

	const chartData = $derived([stats.draft, stats.submitted, stats.approved, stats.rejected, stats.total]);

	$effect(() => {
		if (mounted && chartData) createCharts();
	});
</script>

<div class="card p-6 space-y-6" data-testid="dashboard">
	<div data-testid="dashboard-header">
		<h2 class="text-xl font-bold text-primary">Antrags-Dashboard</h2>
		<p class="text-secondary mt-1" data-testid="dashboard-total">
			Gesamt: {stats.total} Anträge
		</p>
	</div>

	<div
		class="grid grid-cols-2 md:grid-cols-4 gap-4"
		data-testid="dashboard-cards"
	>
		{#each cards as card (card.label)}
			<div
				class="card p-4 flex flex-col items-center gap-2"
				data-testid="dashboard-card-{card.label.toLowerCase()}"
			>
				<div
					class="w-10 h-10 rounded-lg flex items-center justify-center {card.bgClass} {card.colorClass}"
				>
					<card.icon class="w-5 h-5" />
				</div>
				<span class="text-2xl font-bold text-primary" data-testid="dashboard-count-{card.label.toLowerCase()}">{card.count}</span>
				<span class="text-sm text-secondary">{card.label}</span>
			</div>
		{/each}
	</div>

	<div class="grid grid-cols-1 md:grid-cols-2 gap-6" data-testid="dashboard-charts">
		<div class="card p-4" data-testid="dashboard-bar-chart">
			<h3 class="text-sm font-semibold text-primary mb-3">Anträge nach Status</h3>
			<div class="h-52">
				<canvas bind:this={barCanvas}></canvas>
			</div>
		</div>
		<div class="card p-4" data-testid="dashboard-pie-chart">
			<h3 class="text-sm font-semibold text-primary mb-3">Verteilung</h3>
			<div class="h-52">
				<canvas bind:this={pieCanvas}></canvas>
			</div>
		</div>
	</div>
</div>
