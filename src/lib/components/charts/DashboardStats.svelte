<script lang="ts">
	import { FileText, Send, CheckCircle, XCircle } from 'lucide-svelte';
	import type { DashboardStats } from '$lib/server/services/repositories/application.repository';

	let { stats }: { stats: DashboardStats } = $props();

	const cards = $derived([
		{
			label: 'Entwurf',
			value: stats.draft,
			icon: FileText,
			colorClass: 'text-[#3b82f6]',
			bgClass: 'bg-[#3b82f6]/10',
			testId: 'stat-card-draft'
		},
		{
			label: 'Eingereicht',
			value: stats.submitted,
			icon: Send,
			colorClass: 'text-[#6b7280]',
			bgClass: 'bg-[#6b7280]/10',
			testId: 'stat-card-submitted'
		},
		{
			label: 'Genehmigt',
			value: stats.approved,
			icon: CheckCircle,
			colorClass: 'text-[#10b981]',
			bgClass: 'bg-[#10b981]/10',
			testId: 'stat-card-approved'
		},
		{
			label: 'Abgelehnt',
			value: stats.rejected,
			icon: XCircle,
			colorClass: 'text-[#ef4444]',
			bgClass: 'bg-[#ef4444]/10',
			testId: 'stat-card-rejected'
		}
	]);
</script>

<div class="grid grid-cols-2 md:grid-cols-4 gap-4" data-testid="dashboard-stats">
	{#each cards as card}
		<div class="card p-4" data-testid={card.testId}>
			<div class="flex items-center gap-3">
				<div class="w-10 h-10 rounded-lg flex items-center justify-center {card.bgClass}">
					<card.icon class="w-5 h-5 {card.colorClass}" />
				</div>
				<div>
					<p class="text-2xl font-bold {card.colorClass}" data-testid="{card.testId}-value">
						{card.value}
					</p>
					<p class="text-sm text-secondary">{card.label}</p>
				</div>
			</div>
		</div>
	{/each}
</div>
