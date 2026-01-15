<script lang="ts">
	import type { ApplicationStatus } from '$lib/types';
	import { statusLabels } from '$lib/types';

	interface Props {
		status: ApplicationStatus;
	}

	let { status }: Props = $props();

	const statusClasses: Record<ApplicationStatus, string> = {
		draft: 'bg-gray-100 text-gray-800',
		submitted: 'bg-blue-100 text-blue-800',
		approved: 'bg-green-100 text-green-800',
		rejected: 'bg-red-100 text-red-800'
	};
</script>

<span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium {statusClasses[status]}">
	{statusLabels[status]}
</span>
