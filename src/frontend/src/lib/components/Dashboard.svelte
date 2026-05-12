<script lang="ts">
	import type { DashboardStats } from '$lib/types';

	interface Props {
		stats: DashboardStats;
	}

	let { stats }: Props = $props();

	const statusItems = $derived([
		{
			key: 'draft' as const,
			label: 'Entwurf',
			color: '#3B82F6',
			bgColor: '#EFF6FF',
			borderColor: '#BEdBFF',
			icon: 'clock'
		},
		{
			key: 'submitted' as const,
			label: 'Eingereicht',
			color: '#6B7280',
			bgColor: '#F9FAFB',
			borderColor: '#E5E7EB',
			icon: 'document'
		},
		{
			key: 'approved' as const,
			label: 'Genehmigt',
			color: '#10B981',
			bgColor: '#F0FDF4',
			borderColor: '#B9F8CF',
			icon: 'check'
		},
		{
			key: 'rejected' as const,
			label: 'Abgelehnt',
			color: '#EF4444',
			bgColor: '#FEF2F2',
			borderColor: '#FFC9CA',
			icon: 'x'
		}
	]);

	const maxBarValue = $derived(
		Math.max(stats.draft, stats.submitted, stats.approved, stats.rejected, 1)
	);

	const yAxisTicks = $derived(() => {
		const max = maxBarValue;
		const ticks: number[] = [];
		for (let i = max; i >= 0; i--) {
			ticks.push(i);
		}
		return ticks;
	});

	function pieSlices(
		items: {
			key: 'draft' | 'submitted' | 'approved' | 'rejected';
			label: string;
			color: string;
		}[]
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
				class="rounded-[10px] p-5 flex items-center justify-between"
				style="background-color: {item.bgColor}; border: 1.25px solid {item.borderColor};"
				data-testid="dashboard-card-{item.key}"
			>
				<div class="flex flex-col">
					<span class="text-sm text-secondary">{item.label}</span>
					<span
						class="text-3xl font-bold text-primary"
						data-testid="dashboard-count-{item.key}"
					>
						{stats[item.key]}
					</span>
				</div>
				<div class="shrink-0 ml-2" style="color: {item.color};">
					{#if item.icon === 'clock'}
						<svg
							xmlns="http://www.w3.org/2000/svg"
							class="w-10 h-10"
							viewBox="0 0 24 24"
							fill="none"
							stroke="currentColor"
							stroke-width="2"
							stroke-linecap="round"
							stroke-linejoin="round"
						>
							<circle cx="12" cy="12" r="10" />
							<polyline points="12 6 12 12 16 14" />
						</svg>
					{:else if item.icon === 'document'}
						<svg
							xmlns="http://www.w3.org/2000/svg"
							class="w-10 h-10"
							viewBox="0 0 24 24"
							fill="none"
							stroke="currentColor"
							stroke-width="2"
							stroke-linecap="round"
							stroke-linejoin="round"
						>
							<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
							<polyline points="14 2 14 8 20 8" />
							<line x1="16" y1="13" x2="8" y2="13" />
							<line x1="16" y1="17" x2="8" y2="17" />
						</svg>
					{:else if item.icon === 'check'}
						<svg
							xmlns="http://www.w3.org/2000/svg"
							class="w-10 h-10"
							viewBox="0 0 24 24"
							fill="none"
							stroke="currentColor"
							stroke-width="2"
							stroke-linecap="round"
							stroke-linejoin="round"
						>
							<path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
							<polyline points="22 4 12 14.01 9 11.01" />
						</svg>
					{:else if item.icon === 'x'}
						<svg
							xmlns="http://www.w3.org/2000/svg"
							class="w-10 h-10"
							viewBox="0 0 24 24"
							fill="none"
							stroke="currentColor"
							stroke-width="2"
							stroke-linecap="round"
							stroke-linejoin="round"
						>
							<circle cx="12" cy="12" r="10" />
							<line x1="15" y1="9" x2="9" y2="15" />
							<line x1="9" y1="9" x2="15" y2="15" />
						</svg>
					{/if}
				</div>
			</div>
		{/each}
	</div>

	<div class="grid grid-cols-1 md:grid-cols-2 gap-6">
		<div class="card p-6" data-testid="dashboard-bar-chart">
			<h3 class="text-lg font-semibold text-primary mb-4">Antrag nach Status</h3>
			<div class="flex gap-2 h-64">
				<div class="flex flex-col justify-between text-xs text-secondary pr-1 py-1">
					{#each yAxisTicks() as tick}
						<span class="text-right w-4">{tick}</span>
					{/each}
				</div>
				<div class="flex-1 relative">
					<div class="absolute inset-0 flex flex-col justify-between py-1 pointer-events-none">
						{#each yAxisTicks() as gridTick (gridTick)}
							<div class="border-t border-dashed border-gray-200 w-full" aria-hidden="true"></div>
						{/each}
					</div>
					<div class="relative flex items-end justify-around h-full gap-2 pb-6">
						{#each statusItems as item (item.key)}
							<div class="flex flex-col items-center flex-1 h-full justify-end">
								<div
									class="w-full max-w-14 rounded-t transition-all duration-300"
									style="height: {stats.total > 0
										? (stats[item.key] / maxBarValue) * 100
										: 0}%; background-color: {item.color}; min-height: {stats[item.key] > 0
										? '4px'
										: '0'};"
									data-testid="dashboard-bar-{item.key}"
								></div>
							</div>
						{/each}
					</div>
					<div class="flex justify-around">
						{#each statusItems as item (item.key)}
							<span class="text-xs text-secondary text-center flex-1">{item.label}</span>
						{/each}
					</div>
				</div>
			</div>
		</div>

		<div class="card p-6" data-testid="dashboard-pie-chart">
			<h3 class="text-lg font-semibold text-primary mb-4">Verteilung</h3>
			<div class="flex flex-col items-center gap-4">
				<div class="relative">
					<svg viewBox="0 0 300 300" class="w-56 h-56">
						{#if stats.total === 0}
							<circle cx="150" cy="150" r="80" fill="#E5E7EB" />
						{:else}
							<g transform="translate(50, 50)">
								{#each slices as slice}
									<path d={slice.path} fill={slice.color} />
								{/each}
							</g>
							{#each slices as slice}
								{@const labelR = 135}
								{@const rad = (slice.midAngle * Math.PI) / 180}
								{@const lx = 150 + labelR * Math.cos(rad)}
								{@const ly = 150 + labelR * Math.sin(rad)}
								{#if slice.percentage > 0}
									<text
										x={lx}
										y={ly}
										fill={slice.color}
										font-size="11"
										font-weight="500"
										text-anchor="middle"
										dominant-baseline="middle"
									>
										{slice.label}: {slice.percentage}%
									</text>
								{/if}
							{/each}
						{/if}
					</svg>
				</div>
				<div class="flex flex-wrap justify-center gap-x-4 gap-y-1" data-testid="dashboard-pie-legend">
					{#each [...statusItems].reverse() as item (item.key)}
						<span class="inline-flex items-center gap-1.5 text-sm" style="color: {item.color};">
							<span
								class="w-3 h-3 rounded-sm inline-block"
								style="background-color: {item.color};"
							></span>
							{item.label}
						</span>
					{/each}
				</div>
			</div>
		</div>
	</div>
</div>
