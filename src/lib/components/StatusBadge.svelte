<script lang="ts">
	import type { ApplicationStatus } from '$lib/types';
	import { statusLabels } from '$lib/types';

	interface Props {
		status: ApplicationStatus;
	}

	let { status }: Props = $props();
</script>

<span 
	class="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium"
	class:status-draft={status === 'draft'}
	class:status-submitted={status === 'submitted'}
	class:status-approved={status === 'approved'}
	class:status-rejected={status === 'rejected'}
>
	{statusLabels[status]}
</span>
