<script lang="ts">
	import type { DashboardStats } from '$lib/types';
	import { statusEntries } from '$lib/dashboard';
	import StatusCard from './StatusCard.svelte';
	import BarChart from './BarChart.svelte';
	import PieChart from './PieChart.svelte';

	interface Props {
		stats: DashboardStats;
	}

	let { stats }: Props = $props();
</script>

<section class="space-y-6" data-testid="application-dashboard">
	<div data-testid="dashboard-header">
		<h2 class="text-xl font-bold text-primary">Antrags-Dashboard</h2>
		<p class="text-secondary text-sm mt-1">
			Gesamt: {stats.total} {stats.total === 1 ? 'Antrag' : 'Anträge'}
		</p>
	</div>

	<div class="grid grid-cols-2 md:grid-cols-4 gap-4" data-testid="dashboard-status-cards">
		{#each statusEntries as entry}
			<StatusCard
				label={entry.label}
				count={stats[entry.key]}
				color={entry.color}
				bgColor={entry.bgColor}
				borderColor={entry.borderColor}
				testId="dashboard-card-{entry.key}"
			/>
		{/each}
	</div>

	<div class="grid grid-cols-1 md:grid-cols-2 gap-6">
		<BarChart {stats} />
		<PieChart {stats} />
	</div>
</section>
