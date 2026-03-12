<script lang="ts">
	import type { ApplicationStatus } from '$lib/types';
	import { statusLabels } from '$lib/types';

	interface Props {
		status: ApplicationStatus;
	}

	let { status }: Props = $props();
	const statusStyle = $derived(
		status === 'draft'
			? 'background-color: var(--color-bg-muted); color: var(--color-text-muted);'
			: status === 'submitted'
				? 'background-color: color-mix(in srgb, var(--color-info) 15%, transparent); color: var(--color-info);'
				: status === 'approved'
					? 'background-color: color-mix(in srgb, var(--color-success) 15%, transparent); color: var(--color-success);'
					: status === 'rejected'
						? 'background-color: color-mix(in srgb, var(--color-danger) 15%, transparent); color: var(--color-danger);'
						: ''
	);
</script>

<span
	class="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium"
	style={statusStyle}
	data-testid="status-badge-{status}"
>
	{statusLabels[status]}
</span>
