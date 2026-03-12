<script lang="ts">
	import StatusBadge from './StatusBadge.svelte';
	import TrafficLight from './TrafficLight.svelte';
	import { Eye, Edit, Trash2 } from 'lucide-svelte';
	import { employmentStatusLabels, statusLabels, trafficLightLabels } from '$lib/types';
	import type { Application } from '$lib/types';

	const { application, showActions = true, isApplicantView = true, onView, onEdit, onDelete } =
		$props<{
			application: Application;
			showActions?: boolean;
			isApplicantView?: boolean;
			onView?: (id: number) => void;
			onEdit?: (id: number) => void;
			onDelete?: (id: number) => void;
		}>();

	const formatDate = (value: string | null) => {
		if (!value) return '-';
		return new Date(value).toLocaleDateString('de-DE', {
			day: '2-digit',
			month: '2-digit',
			year: 'numeric',
			hour: '2-digit',
			minute: '2-digit'
		});
	};

	const formatCurrency = (value: number | null | undefined) => {
		if (value === null || value === undefined) return '-';
		return new Intl.NumberFormat('de-DE', {
			style: 'currency',
			currency: 'EUR'
		}).format(value);
	};
</script>

<div class="mobile-card" data-testid={`application-card-${application.id}`}>
	<div class="mobile-card-header">
		<div class="flex-1 min-w-0">
			<div class="text-base font-semibold text-primary truncate" title={application.name}>
				{application.name}
			</div>
		</div>
		<StatusBadge status={application.status} />
	</div>

	<div class="mobile-card-body">
		<div class="space-y-1">
			<div class="text-xs text-secondary">Beschäftigungsstatus</div>
			<div class="text-sm text-primary">
				{application.employmentStatus ? employmentStatusLabels[application.employmentStatus] : '-'}
			</div>
		</div>
		<div class="space-y-1">
			<div class="text-xs text-secondary">Score</div>
			<div class="text-sm font-medium">
				{application.score ?? '-'}
			</div>
		</div>
		<div class="space-y-1">
			<div class="text-xs text-secondary">Ampel</div>
			<div class="flex items-center gap-2">
				<TrafficLight status={application.trafficLight} showLabel={false} />
				<span class="text-sm text-secondary">{application.trafficLight ? trafficLightLabels[application.trafficLight] : '-'}</span>
			</div>
		</div>
		<div class="space-y-1">
			<div class="text-xs text-secondary">Rate</div>
			<div class="text-sm text-primary">{formatCurrency(application.desiredRate)}</div>
		</div>
		<div class="space-y-1">
			<div class="text-xs text-secondary">Erstellt</div>
			<div class="text-sm text-primary">{formatDate(application.createdAt)}</div>
		</div>
		<div class="space-y-1">
			<div class="text-xs text-secondary">Status</div>
			<div class="text-sm text-primary">{statusLabels[application.status] ?? application.status}</div>
		</div>
	</div>

	{#if showActions}
		<div class="mobile-card-actions">
			<button
				type="button"
				class="inline-flex items-center justify-center gap-2 px-3 py-2 min-h-[44px] min-w-[44px] rounded-md border border-border text-primary hover:text-brand-primary"
				onclick={() => onView?.(application.id)}
				data-testid={`card-view-btn-${application.id}`}
			>
				<Eye class="w-4 h-4" />
				<span>Ansehen</span>
			</button>
			{#if isApplicantView && application.status === 'draft'}
				<div class="flex flex-wrap gap-2">
					<button
						type="button"
						class="inline-flex items-center justify-center gap-2 px-3 py-2 min-h-[44px] min-w-[44px] rounded-md border border-border text-secondary hover:text-primary"
						onclick={() => onEdit?.(application.id)}
						data-testid={`card-edit-btn-${application.id}`}
					>
						<Edit class="w-4 h-4" />
						<span>Bearbeiten</span>
					</button>
					<button
						type="button"
						class="inline-flex items-center justify-center gap-2 px-3 py-2 min-h-[44px] min-w-[44px] rounded-md border border-danger text-danger hover:opacity-80"
						onclick={() => onDelete?.(application.id)}
						data-testid={`card-delete-btn-${application.id}`}
					>
						<Trash2 class="w-4 h-4" />
						<span>Löschen</span>
					</button>
				</div>
			{/if}
		</div>
	{/if}
</div>
