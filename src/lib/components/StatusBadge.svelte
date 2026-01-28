<script lang="ts">
	import type { ApplicationStatus } from '$lib/types';
	import { statusLabels } from '$lib/types';

	interface Props {
		status: ApplicationStatus;
	}

	let { status }: Props = $props();
	const statusClass = $derived(() => {
		switch (status) {
			case 'draft':
				return 'bg-bg-muted text-text-muted';
			case 'submitted':
				return 'bg-info/15 text-info';
			case 'approved':
				return 'bg-success/15 text-success';
			case 'rejected':
				return 'bg-danger/15 text-danger';
			default:
				return '';
		}
	});
</script>

<span
	class={`inline-flex items-center px-3 py-1 rounded-full text-xs font-medium ${statusClass}`}
	data-testid="status-badge-{status}"
>
	{statusLabels[status]}
</span>
