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

<div class="overflow-x-auto">
	<table class="min-w-full divide-y divide-gray-200">
		<thead class="bg-gray-50">
			<tr>
				<th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Name
				</th>
				<th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Status
				</th>
				<th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Score
				</th>
				<th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Ampel
				</th>
				<th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Rate
				</th>
				<th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Erstellt
				</th>
				{#if showActions}
					<th scope="col" class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
						Aktionen
					</th>
				{/if}
			</tr>
		</thead>
		<tbody class="bg-white divide-y divide-gray-200">
			{#each applications as app}
				<tr class="hover:bg-gray-50">
					<td class="px-6 py-4 whitespace-nowrap">
						<div class="text-sm font-medium text-gray-900">{app.name}</div>
						<div class="text-sm text-gray-500">{employmentStatusLabels[app.employmentStatus]}</div>
					</td>
					<td class="px-6 py-4 whitespace-nowrap">
						<StatusBadge status={app.status} />
					</td>
					<td class="px-6 py-4 whitespace-nowrap">
						<span class="text-sm font-medium {app.score !== null ? (app.score >= 75 ? 'text-green-600' : app.score >= 50 ? 'text-yellow-600' : 'text-red-600') : 'text-gray-400'}">
							{app.score !== null ? app.score : '-'}
						</span>
					</td>
					<td class="px-6 py-4 whitespace-nowrap">
						<TrafficLight status={app.trafficLight} showLabel={false} />
					</td>
					<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
						{formatCurrency(app.desiredRate)}
					</td>
					<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
						{formatDate(app.createdAt)}
					</td>
					{#if showActions}
						<td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
							<div class="flex justify-end gap-2">
								<button
									onclick={() => onView?.(app.id)}
									class="text-indigo-600 hover:text-indigo-900 p-1"
									title="Ansehen"
								>
									<Eye class="w-4 h-4" />
								</button>
								{#if isApplicantView && app.status === 'draft'}
									<button
										onclick={() => onEdit?.(app.id)}
										class="text-gray-600 hover:text-gray-900 p-1"
										title="Bearbeiten"
									>
										<Edit class="w-4 h-4" />
									</button>
									<button
										onclick={() => onDelete?.(app.id)}
										class="text-red-600 hover:text-red-900 p-1"
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
					<td colspan={showActions ? 7 : 6} class="px-6 py-8 text-center text-gray-500">
						Keine Anträge vorhanden
					</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>
