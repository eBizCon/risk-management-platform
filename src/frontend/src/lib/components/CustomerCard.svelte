<script lang="ts">
	import { customerStatusLabels } from '$lib/types';
	import type { Customer } from '$lib/types';
	import { Eye, Edit, Trash2, Archive, RotateCcw } from 'lucide-svelte';

	const { customer, showActions = true, onView, onEdit, onDelete, onArchive, onActivate } =
		$props<{
			customer: Customer;
			showActions?: boolean;
			onView?: (id: number) => void;
			onEdit?: (id: number) => void;
			onDelete?: (id: number) => void;
			onArchive?: (id: number) => void;
			onActivate?: (id: number) => void;
		}>();

	const formatDate = (value: string | null) => {
		if (!value) return '-';
		return new Date(value).toLocaleDateString('de-DE', {
			day: '2-digit',
			month: '2-digit',
			year: 'numeric'
		});
	};
</script>

<div class="mobile-card" data-testid={`customer-card-${customer.id}`}>
	<div class="mobile-card-header">
		<div class="flex-1 min-w-0">
			<div class="text-base font-semibold text-primary truncate" title="{customer.lastName}, {customer.firstName}">
				{customer.lastName}, {customer.firstName}
			</div>
			<div class="text-sm text-secondary truncate">{customer.email ?? '-'}</div>
		</div>
		<span
			class="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium shrink-0"
			class:bg-success={customer.status === 'active'}
			class:text-white={customer.status === 'active'}
			class:bg-bg-muted={customer.status === 'archived'}
			class:text-secondary={customer.status === 'archived'}
		>
			{customerStatusLabels[customer.status] ?? customer.status}
		</span>
	</div>

	<div class="mobile-card-body">
		<div class="space-y-1">
			<div class="text-xs text-secondary">Telefon</div>
			<div class="text-sm text-primary">{customer.phone}</div>
		</div>
		<div class="space-y-1">
			<div class="text-xs text-secondary">Ort</div>
			<div class="text-sm text-primary">{customer.zipCode} {customer.city}</div>
		</div>
		<div class="space-y-1">
			<div class="text-xs text-secondary">Erstellt</div>
			<div class="text-sm text-primary">{formatDate(customer.createdAt)}</div>
		</div>
	</div>

	{#if showActions}
		<div class="mobile-card-actions">
			<button
				type="button"
				class="inline-flex items-center justify-center gap-2 px-3 py-2 min-h-[44px] min-w-[44px] rounded-md border border-border text-primary hover:text-brand-primary"
				onclick={() => onView?.(customer.id)}
				data-testid={`customer-card-view-btn-${customer.id}`}
			>
				<Eye class="w-4 h-4" />
				<span>Ansehen</span>
			</button>
			<div class="flex flex-wrap gap-2">
				{#if customer.status === 'active'}
					<button
						type="button"
						class="inline-flex items-center justify-center gap-2 px-3 py-2 min-h-[44px] min-w-[44px] rounded-md border border-border text-secondary hover:text-primary"
						onclick={() => onEdit?.(customer.id)}
						data-testid={`customer-card-edit-btn-${customer.id}`}
					>
						<Edit class="w-4 h-4" />
						<span>Bearbeiten</span>
					</button>
					<button
						type="button"
						class="inline-flex items-center justify-center gap-2 px-3 py-2 min-h-[44px] min-w-[44px] rounded-md border border-warning text-warning hover:opacity-80"
						onclick={() => onArchive?.(customer.id)}
						data-testid={`customer-card-archive-btn-${customer.id}`}
					>
						<Archive class="w-4 h-4" />
						<span>Archivieren</span>
					</button>
				{:else}
					<button
						type="button"
						class="inline-flex items-center justify-center gap-2 px-3 py-2 min-h-[44px] min-w-[44px] rounded-md border border-success text-success hover:opacity-80"
						onclick={() => onActivate?.(customer.id)}
						data-testid={`customer-card-activate-btn-${customer.id}`}
					>
						<RotateCcw class="w-4 h-4" />
						<span>Aktivieren</span>
					</button>
				{/if}
				<button
					type="button"
					class="inline-flex items-center justify-center gap-2 px-3 py-2 min-h-[44px] min-w-[44px] rounded-md border border-danger text-danger hover:opacity-80"
					onclick={() => onDelete?.(customer.id)}
					data-testid={`customer-card-delete-btn-${customer.id}`}
				>
					<Trash2 class="w-4 h-4" />
					<span>Löschen</span>
				</button>
			</div>
		</div>
	{/if}
</div>
