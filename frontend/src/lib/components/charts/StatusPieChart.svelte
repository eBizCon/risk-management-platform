<script lang="ts">
	import { ChartJS } from './chartjs-register';
	import type { DashboardStats } from '$lib/types';

	let { stats }: { stats: DashboardStats } = $props();

	let canvas: HTMLCanvasElement;
	let chart: InstanceType<typeof ChartJS> | undefined;

	const labels = ['Entwurf', 'Eingereicht', 'Genehmigt', 'Abgelehnt'];
	const colors = ['#3b82f6', '#6b7280', '#10b981', '#ef4444'];

	$effect(() => {
		if (!canvas) return;

		const data = [stats.draft, stats.submitted, stats.approved, stats.rejected];
		const total = data.reduce((sum, v) => sum + v, 0);

		if (chart) {
			chart.data.datasets[0].data = data;
			chart.update();
			return;
		}

		chart = new ChartJS(canvas, {
			type: 'pie',
			data: {
				labels,
				datasets: [
					{
						data,
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
							pointStyle: 'rect',
							padding: 16,
							color: '#666',
							font: { size: 14 }
						}
					},
					tooltip: {
						callbacks: {
							label: (context) => {
								const value = context.parsed;
								const pct = total > 0 ? Math.round((value / total) * 100) : 0;
								return `${context.label}: ${pct}%`;
							}
						}
					}
				}
			},
			plugins: [
				{
					id: 'pieLabels',
					afterDraw(chartInstance) {
						const { ctx } = chartInstance;
						const dataset = chartInstance.data.datasets[0];
						const meta = chartInstance.getDatasetMeta(0);
						const currentTotal = (dataset.data as number[]).reduce(
							(sum: number, v: number) => sum + v,
							0
						);
						if (currentTotal === 0) return;

						ctx.save();
						ctx.font = '12px Inter, system-ui, sans-serif';
						ctx.textAlign = 'center';
						ctx.textBaseline = 'middle';

						meta.data.forEach((element, index) => {
							const value = dataset.data[index] as number;
							if (value === 0) return;
							const pct = Math.round((value / currentTotal) * 100);
							const label = chartInstance.data.labels?.[index] as string;

							const pos = element.tooltipPosition(false);
							const arc = element as unknown as { startAngle: number; endAngle: number };
							const midAngle = (arc.startAngle + arc.endAngle) / 2;
							const offsetX = Math.cos(midAngle) * 30;
							const offsetY = Math.sin(midAngle) * 30;

							ctx.fillStyle = '#ffffff';
							ctx.strokeStyle = '#000000';
							ctx.lineWidth = 1;
							ctx.strokeText(`${label}: ${pct}%`, (pos.x ?? 0) + offsetX, (pos.y ?? 0) + offsetY);
							ctx.fillText(`${label}: ${pct}%`, (pos.x ?? 0) + offsetX, (pos.y ?? 0) + offsetY);
						});

						ctx.restore();
					}
				}
			]
		});

		return () => {
			chart?.destroy();
			chart = undefined;
		};
	});
</script>

<div class="card p-6" data-testid="dashboard-pie-chart">
	<h3 class="text-xl font-semibold text-primary mb-4">Verteilung</h3>
	<div class="h-[300px]">
		<canvas bind:this={canvas}></canvas>
	</div>
</div>
