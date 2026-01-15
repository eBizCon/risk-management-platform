<script lang="ts">
	import type { TrafficLight } from '$lib/types';
	import { trafficLightLabels } from '$lib/types';

	interface Props {
		status: TrafficLight | null;
		showLabel?: boolean;
	}

	let { status, showLabel = true }: Props = $props();

	const colorClasses: Record<TrafficLight, string> = {
		green: 'bg-green-500',
		yellow: 'bg-yellow-500',
		red: 'bg-red-500'
	};
</script>

{#if status}
	<div class="flex items-center gap-2">
		<div class="flex gap-1">
			<div
				class="w-4 h-4 rounded-full {status === 'red' ? colorClasses.red : 'bg-gray-300'}"
			></div>
			<div
				class="w-4 h-4 rounded-full {status === 'yellow' ? colorClasses.yellow : 'bg-gray-300'}"
			></div>
			<div
				class="w-4 h-4 rounded-full {status === 'green' ? colorClasses.green : 'bg-gray-300'}"
			></div>
		</div>
		{#if showLabel}
			<span class="text-sm font-medium">{trafficLightLabels[status]}</span>
		{/if}
	</div>
{:else}
	<span class="text-gray-400 text-sm">-</span>
{/if}
