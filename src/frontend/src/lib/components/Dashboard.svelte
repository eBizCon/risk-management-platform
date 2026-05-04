<script lang="ts">
	import type { DashboardStats } from '$lib/types';

	interface Props {
		stats: DashboardStats;
	}

	let { stats }: Props = $props();

	const statusItems = $derived([
		{ key: 'draft' as const, label: 'Entwurf', color: '#3B82F6', bgClass: 'bg-blue-50', textClass: 'text-blue-600' },
		{
			key: 'submitted' as const,
			label: 'Eingereicht',
			color: '#6B7280',
			bgClass: 'bg-gray-50',
			textClass: 'text-gray-600'
		},
		{
			key: 'approved' as const,
			label: 'Genehmigt',
			color: '#10B981',
			bgClass: 'bg-emerald-50',
			textClass: 'text-emerald-600'
		},
		{
			key: 'rejected' as const,
			label: 'Abgelehnt',
			color: '#EF4444',
			bgClass: 'bg-red-50',
			textClass: 'text-red-600'
		}
	]);

	const maxBarValue = $derived(Math.max(stats.draft, stats.submitted, stats.approved, stats.rejected, 1));

	function pieSlices(
		items: { key: 'draft' | 'submitted' | 'approved' | 'rejected'; label: string; color: string }[]
	): { path: string; color: string; label: string; percentage: number; midAngle: number }[] {
		if (stats.total === 0) return [];

		const slices: {
			path: string;
			color: string;
			label: string;
			percentage: number;
			midAngle: number;
		}[] = [];
		let currentAngle = -90;
		const cx = 100;
		const cy = 100;
		const r = 80;

		for (const item of items) {
			const value = stats[item.key];
			if (value === 0) continue;
			const percentage = Math.round((value / stats.total) * 100);
			const angle = (value / stats.total) * 360;
			const midAngle = currentAngle + angle / 2;
			const startRad = (currentAngle * Math.PI) / 180;
			const endRad = ((currentAngle + angle) * Math.PI) / 180;

			const x1 = cx + r * Math.cos(startRad);
			const y1 = cy + r * Math.sin(startRad);
			const x2 = cx + r * Math.cos(endRad);
			const y2 = cy + r * Math.sin(endRad);
			const largeArc = angle > 180 ? 1 : 0;

			const path = `M ${cx} ${cy} L ${x1} ${y1} A ${r} ${r} 0 ${largeArc} 1 ${x2} ${y2} Z`;
			slices.push({ path, color: item.color, label: item.label, percentage, midAngle });
			currentAngle += angle;
		}

		return slices;
	}

	const slices = $derived(pieSlices(statusItems));
</script>

<div class="space-y-6" data-testid="dashboard">
	<div>
		<h2 class="text-2xl sm:text-3xl font-bold text-primary" data-testid="dashboard-heading">
			Antrags-Dashboard
		</h2>
		<p class="text-secondary mt-1" data-testid="dashboard-total">
			Gesamt: {stats.total} Anträge
		</p>
	</div>

	<div class="grid grid-cols-2 sm:grid-cols-4 gap-4" data-testid="dashboard-status-cards">
		{#each statusItems as item (item.key)}
			<div
				class="card p-4 flex flex-col items-start"
				data-testid="dashboard-card-{item.key}"
			>
				<span class="text-sm text-secondary">{item.label}</span>
				<span class="text-2xl font-bold text-primary" data-testid="dashboard-count-{item.key}">
					{stats[item.key]}
				</span>
			</div>
		{/each}
	</div>

	<div class="grid grid-cols-1 md:grid-cols-2 gap-6">
		<div class="card p-6" data-testid="dashboard-bar-chart">
			<h3 class="text-lg font-semibold text-primary mb-4">Antrag nach Status</h3>
			<div class="space-y-3">
				{#each statusItems as item (item.key)}
					<div class="flex items-center gap-3">
						<span class="text-sm text-secondary w-24 shrink-0">{item.label}</span>
						<div class="flex-1 bg-gray-100 rounded-full h-6 overflow-hidden">
							<div
								class="h-full rounded-full transition-all duration-300"
								style="width: {stats.total > 0
									? (stats[item.key] / maxBarValue) * 100
									: 0}%; background-color: {item.color};"
								data-testid="dashboard-bar-{item.key}"
							></div>
						</div>
						<span class="text-sm font-medium text-primary w-8 text-right">{stats[item.key]}</span>
					</div>
				{/each}
			</div>
		</div>

		<div class="card p-6" data-testid="dashboard-pie-chart">
			<h3 class="text-lg font-semibold text-primary mb-4">Verteilung</h3>
			<div class="flex flex-col items-center gap-4">
				<svg viewBox="0 0 200 200" class="w-48 h-48">
					{#if stats.total === 0}
						<circle cx="100" cy="100" r="80" fill="#E5E7EB" />
					{:else}
						{#each slices as slice}
							<path d={slice.path} fill={slice.color} />
						{/each}
					{/if}
				</svg>
				<div class="flex flex-wrap justify-center gap-x-4 gap-y-1" data-testid="dashboard-pie-legend">
					{#each statusItems as item (item.key)}
						{@const value = stats[item.key]}
						{#if value > 0}
							<span class="inline-flex items-center gap-1.5 text-sm" style="color: {item.color};">
								<span
									class="w-2.5 h-2.5 rounded-full inline-block"
									style="background-color: {item.color};"
								></span>
								{item.label}: {Math.round((value / stats.total) * 100)}%
							</span>
						{/if}
					{/each}
				</div>
			</div>
		</div>
	</div>
</div>
