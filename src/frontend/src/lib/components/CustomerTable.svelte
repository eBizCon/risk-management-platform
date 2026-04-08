<script lang="ts">
	import type { Customer } from '$lib/types';
	import { customerStatusLabels } from '$lib/types';
	import { Eye, Edit, Trash2, Archive, RotateCcw } from 'lucide-svelte';
	import CustomerCard from './CustomerCard.svelte';

	interface Props {
		customers: Customer[];
		showActions?: boolean;
		onView?: (id: number) => void;
		onEdit?: (id: number) => void;
		onDelete?: (id: number) => void;
		onArchive?: (id: number) => void;
		onActivate?: (id: number) => void;
	}

	let {
		customers,
		showActions = true,
		onView,
		onEdit,
		onDelete,
		onArchive,
		onActivate
	}: Props = $props();

	function formatDate(dateString: string | null): string {
		if (!dateString) return '-';
		return new Date(dateString).toLocaleDateString('de-DE', {
			day: '2-digit',
			month: '2-digit',
			year: 'numeric'
		});
	}
</script>

<div class="block sm:hidden space-y-3">
	{#if customers.length === 0}
		<div class="text-center text-secondary">Keine Kunden vorhanden</div>
	{:else}
		{#each customers as customer}
			<CustomerCard
				{customer}
				{showActions}
				{onView}
				{onEdit}
				{onDelete}
				{onArchive}
				{onActivate}
			/>
		{/each}
	{/if}
</div>

<div class="hidden sm:block overflow-x-auto" data-testid="customer-table">
	<table class="min-w-full divide-y border-default">
		<thead class="table-header">
			<tr>
				<th scope="col" class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider">
					Name
				</th>
				<th scope="col" class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider">
					E-Mail
				</th>
				<th scope="col" class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider">
					Telefon
				</th>
				<th scope="col" class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider">
					Ort
				</th>
				<th scope="col" class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider">
					Status
				</th>
				<th scope="col" class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider">
					Erstellt
				</th>
				{#if showActions}
					<th scope="col" class="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider">
						Aktionen
					</th>
				{/if}
			</tr>
		</thead>
		<tbody class="surface-bg divide-y border-default">
			{#each customers as customer}
				<tr class="table-row" data-testid="customer-row-{customer.id}">
					<td class="px-4 py-4 whitespace-nowrap">
						<div class="text-sm font-medium text-primary" data-testid="customer-name-{customer.id}">
							{customer.lastName}, {customer.firstName}
						</div>
					</td>
					<td class="px-4 py-4 whitespace-nowrap text-sm text-secondary">
						{customer.email ?? '-'}
					</td>
					<td class="px-4 py-4 whitespace-nowrap text-sm text-secondary">
						{customer.phone}
					</td>
					<td class="px-4 py-4 whitespace-nowrap text-sm text-secondary">
						{customer.zipCode}
						{customer.city}
					</td>
					<td class="px-4 py-4 whitespace-nowrap">
						<span
							class="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium"
							class:bg-success={customer.status === 'active'}
							class:text-white={customer.status === 'active'}
							class:bg-bg-muted={customer.status === 'archived'}
							class:text-secondary={customer.status === 'archived'}
							data-testid="customer-status-{customer.id}"
						>
							{customerStatusLabels[customer.status] ?? customer.status}
						</span>
					</td>
					<td class="px-4 py-4 whitespace-nowrap text-sm text-secondary">
						{formatDate(customer.createdAt)}
					</td>
					{#if showActions}
						<td class="px-4 py-4 whitespace-nowrap text-right text-sm font-medium">
							<div class="flex justify-end gap-2">
								<button
									onclick={() => onView?.(customer.id)}
									class="text-brand-primary hover:text-brand-primary-hover p-1"
									title="Ansehen"
									data-testid="customer-view-btn-{customer.id}"
								>
									<Eye class="w-4 h-4" />
								</button>
								{#if customer.status === 'active'}
									<button
										onclick={() => onEdit?.(customer.id)}
										class="text-text-muted hover:text-text p-1"
										title="Bearbeiten"
										data-testid="customer-edit-btn-{customer.id}"
									>
										<Edit class="w-4 h-4" />
									</button>
									<button
										onclick={() => onArchive?.(customer.id)}
										class="text-warning hover:opacity-80 p-1"
										title="Archivieren"
										data-testid="customer-archive-btn-{customer.id}"
									>
										<Archive class="w-4 h-4" />
									</button>
								{:else}
									<button
										onclick={() => onActivate?.(customer.id)}
										class="text-success hover:opacity-80 p-1"
										title="Aktivieren"
										data-testid="customer-activate-btn-{customer.id}"
									>
										<RotateCcw class="w-4 h-4" />
									</button>
								{/if}
								<button
									onclick={() => onDelete?.(customer.id)}
									class="text-danger hover:opacity-80 p-1"
									title="Löschen"
									data-testid="customer-delete-btn-{customer.id}"
								>
									<Trash2 class="w-4 h-4" />
								</button>
							</div>
						</td>
					{/if}
				</tr>
			{:else}
				<tr>
					<td colspan={showActions ? 7 : 6} class="px-4 py-8 text-center text-secondary">
						Keine Kunden vorhanden
					</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>
