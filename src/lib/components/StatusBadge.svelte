<script lang="ts">
	import type { ApplicationStatus } from '$lib/types';
	import { statusLabels } from '$lib/types';

	interface Props {
		status: ApplicationStatus;
	}

	let { status }: Props = $props();
</script>

<span
	class={`inline-flex items-center px-3 py-1 rounded-full text-xs font-medium status-${status}`}
	data-testid="status-badge-{status}"
>
	{statusLabels[status]}
</span>
