<script lang="ts">
	import StatusBarChart from './StatusBarChart.svelte';
	import StatusPieChart from './StatusPieChart.svelte';
	import type { DashboardStats } from '$lib/server/services/repositories/application.repository';

	let { stats }: { stats: DashboardStats } = $props();
</script>

<div class="grid grid-cols-1 md:grid-cols-2 gap-6" data-testid="dashboard-charts">
	<StatusBarChart {stats} />
	<StatusPieChart {stats} />
</div>
