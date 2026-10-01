<script lang="ts">
	import { onMount } from 'svelte';
	import { Chart, PieController, ArcElement, Tooltip, Legend } from 'chart.js';
	import type { DashboardStats } from '$lib/types';
	import { statusEntries, computePercentages } from '$lib/dashboard';

	Chart.register(PieController, ArcElement, Tooltip, Legend);

	interface Props {
		stats: DashboardStats;
	}

	let { stats }: Props = $props();

	let canvas: HTMLCanvasElement;
	let chart: Chart | null = null;

	function buildLabels(): string[] {
		const pcts = computePercentages(stats);
		return statusEntries.map((e) => {
			if (stats.total === 0 || stats[e.key] === 0) return e.label;
			return `${e.label}: ${pcts[e.key]}%`;
		});
	}

	function createChart() {
		if (chart) chart.destroy();

		chart = new Chart(canvas, {
			type: 'pie',
			data: {
				labels: buildLabels(),
				datasets: [
					{
						data: statusEntries.map((e) => stats[e.key]),
						backgroundColor: statusEntries.map((e) => e.color),
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
						position: 'right',
						labels: {
							usePointStyle: true,
							pointStyle: 'circle',
							padding: 16,
							font: { size: 13 }
						}
					},
					tooltip: { enabled: true }
				}
			}
		});
	}

	onMount(() => {
		createChart();
		return () => {
			if (chart) chart.destroy();
		};
	});

	$effect(() => {
		if (chart && stats) {
			chart.data.labels = buildLabels();
			chart.data.datasets[0].data = statusEntries.map((e) => stats[e.key]);
			chart.update();
		}
	});
</script>

<div class="card p-6" data-testid="dashboard-pie-chart">
	<h3 class="text-base font-semibold text-primary mb-4">Verteilung</h3>
	<div class="h-64">
		<canvas bind:this={canvas}></canvas>
	</div>
</div>
