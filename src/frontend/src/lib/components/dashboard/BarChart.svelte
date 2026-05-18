<script lang="ts">
	import { onMount } from 'svelte';
	import { Chart, BarController, CategoryScale, LinearScale, BarElement, Tooltip } from 'chart.js';
	import type { DashboardStats } from '$lib/types';
	import { statusEntries } from '$lib/dashboard';

	Chart.register(BarController, CategoryScale, LinearScale, BarElement, Tooltip);

	interface Props {
		stats: DashboardStats;
	}

	let { stats }: Props = $props();

	let canvas: HTMLCanvasElement;
	let chart: Chart | null = null;

	function createChart() {
		if (chart) chart.destroy();

		chart = new Chart(canvas, {
			type: 'bar',
			data: {
				labels: statusEntries.map((e) => e.label),
				datasets: [
					{
						data: statusEntries.map((e) => stats[e.key]),
						backgroundColor: statusEntries.map((e) => e.color),
						borderRadius: 4,
						maxBarThickness: 60
					}
				]
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
						ticks: { stepSize: 1, precision: 0 },
						grid: { color: '#e5e7eb' }
					},
					x: {
						grid: { display: false }
					}
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
			chart.data.datasets[0].data = statusEntries.map((e) => stats[e.key]);
			chart.update();
		}
	});
</script>

<div class="card p-6" data-testid="dashboard-bar-chart">
	<h3 class="text-base font-semibold text-primary mb-4">Anträge nach Status</h3>
	<div class="h-64">
		<canvas bind:this={canvas}></canvas>
	</div>
</div>
