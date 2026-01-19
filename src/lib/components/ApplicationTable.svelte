<script lang="ts">
	import type { Application } from '$lib/types';
	import { statusLabels, employmentStatusLabels } from '$lib/types';
	import StatusBadge from './StatusBadge.svelte';
	import TrafficLight from './TrafficLight.svelte';
	import { Eye, Edit, Trash2 } from 'lucide-svelte';

	interface Props {
		applications: Application[];
		showActions?: boolean;
		isApplicantView?: boolean;
		onView?: (id: number) => void;
		onEdit?: (id: number) => void;
		onDelete?: (id: number) => void;
	}

	let { 
		applications, 
		showActions = true, 
		isApplicantView = true,
		onView,
		onEdit,
		onDelete
	}: Props = $props();

	function formatDate(dateString: string | null): string {
		if (!dateString) return '-';
		return new Date(dateString).toLocaleDateString('de-DE', {
			day: '2-digit',
			month: '2-digit',
			year: 'numeric',
			hour: '2-digit',
			minute: '2-digit'
		});
	}

	function formatCurrency(value: number): string {
		return new Intl.NumberFormat('de-DE', {
			style: 'currency',
			currency: 'EUR'
		}).format(value);
	}
</script>

<div class="overflow-x-auto" data-testid="application-table">
	<table class="min-w-full divide-y border-default">
		<thead class="table-header">
			<tr>
				<th scope="col" class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider">
					Name
				</th>
				<th scope="col" class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider">
					Status
				</th>
				<th scope="col" class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider">
					Score
				</th>
				<th scope="col" class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider">
					Ampel
				</th>
				<th scope="col" class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider">
					Rate
				</th>
				<th scope="col" class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider">
					Erstellt
				</th>
				{#if showActions}
					<th scope="col" class="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider">
						Aktionen
					</th>
				{/if}
			</tr>
		</thead>
		<tbody class="surface-bg divide-y border-default">
			{#each applications as app}
				<tr class="table-row" data-testid="application-row-{app.id}">
					<td class="px-6 py-4 whitespace-nowrap">
						<div class="text-sm font-medium text-primary" data-testid="application-name-{app.id}">{app.name}</div>
						<div class="text-sm text-secondary">{employmentStatusLabels[app.employmentStatus]}</div>
					</td>
					<td class="px-6 py-4 whitespace-nowrap">
						<StatusBadge status={app.status} />
					</td>
					<td class="px-6 py-4 whitespace-nowrap">
						<span class="text-sm font-medium" class:score-high={app.score !== null && app.score >= 75} class:score-medium={app.score !== null && app.score >= 50 && app.score < 75} class:score-low={app.score !== null && app.score < 50} class:text-secondary={app.score === null}>
							{app.score !== null ? app.score : '-'}
						</span>
					</td>
					<td class="px-6 py-4 whitespace-nowrap">
						<TrafficLight status={app.trafficLight} showLabel={false} />
					</td>
					<td class="px-6 py-4 whitespace-nowrap text-sm text-secondary">
						{formatCurrency(app.desiredRate)}
					</td>
					<td class="px-6 py-4 whitespace-nowrap text-sm text-secondary">
						{formatDate(app.createdAt)}
					</td>
					{#if showActions}
						<td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
							<div class="flex justify-end gap-2">
								<button
									onclick={() => onView?.(app.id)}
									class="action-btn-view p-1"
									title="Ansehen"
								>
									<Eye class="w-4 h-4" />
								</button>
								{#if isApplicantView && app.status === 'draft'}
									<button
										onclick={() => onEdit?.(app.id)}
										class="action-btn-edit p-1"
										title="Bearbeiten"
									>
										<Edit class="w-4 h-4" />
									</button>
									<button
										onclick={() => onDelete?.(app.id)}
										class="action-btn-delete p-1"
										title="Löschen"
									>
										<Trash2 class="w-4 h-4" />
									</button>
								{/if}
							</div>
						</td>
					{/if}
				</tr>
			{:else}
				<tr>
					<td colspan={showActions ? 7 : 6} class="px-6 py-8 text-center text-secondary">
						Keine Anträge vorhanden
					</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<style>
	.action-btn-view {
		color: var(--brand-primary);
	}
	.action-btn-view:hover {
		color: var(--brand-primary-hover);
	}
	.action-btn-edit {
		color: var(--text-muted);
	}
	.action-btn-edit:hover {
		color: var(--text);
	}
	.action-btn-delete {
		color: var(--danger);
	}
	.action-btn-delete:hover {
		opacity: 0.8;
	}
</style>
