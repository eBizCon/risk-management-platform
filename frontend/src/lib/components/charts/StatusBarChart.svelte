<script lang="ts">
	import { ChartJS } from './chartjs-register';
	import type { DashboardStats } from '$lib/server/services/repositories/application.repository';

	let { stats }: { stats: DashboardStats } = $props();

	let canvas: HTMLCanvasElement;
	let chart: InstanceType<typeof ChartJS> | undefined;

	const labels = ['Entwurf', 'Eingereicht', 'Genehmigt', 'Abgelehnt'];
	const colors = ['#3b82f6', '#6b7280', '#10b981', '#ef4444'];

	$effect(() => {
		if (!canvas) return;

		const data = [stats.draft, stats.submitted, stats.approved, stats.rejected];

		if (chart) {
			chart.data.datasets[0].data = data;
			chart.update();
			return;
		}

		chart = new ChartJS(canvas, {
			type: 'bar',
			data: {
				labels,
				datasets: [
					{
						data,
						backgroundColor: colors,
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
					title: { display: false }
				},
				scales: {
					y: {
						beginAtZero: true,
						ticks: {
							stepSize: 1,
							color: '#666'
						},
						grid: {
							color: '#e5e7eb'
						}
					},
					x: {
						ticks: {
							color: '#666'
						},
						grid: {
							display: false
						}
					}
				}
			}
		});

		return () => {
			chart?.destroy();
			chart = undefined;
		};
	});
</script>

<div class="card p-6" data-testid="dashboard-bar-chart">
	<h3 class="text-xl font-semibold text-primary mb-4">Antrag nach Status</h3>
	<div class="h-[300px]">
		<canvas bind:this={canvas}></canvas>
	</div>
</div>
