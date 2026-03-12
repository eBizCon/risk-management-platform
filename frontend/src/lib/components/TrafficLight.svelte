<script lang="ts">
	import type { TrafficLight } from '$lib/types';
	import { trafficLightLabels } from '$lib/types';

	interface Props {
		status: TrafficLight | null;
		showLabel?: boolean;
	}

	let { status, showLabel = true }: Props = $props();
</script>

{#if status}
	<div class="flex items-center gap-2" data-testid="traffic-light">
		<div class="flex gap-1">
			<div
				class="traffic-indicator"
				class:traffic-red={status === 'red'}
				class:traffic-inactive={status !== 'red'}
				data-testid="traffic-light-red"
			></div>
			<div
				class="traffic-indicator"
				class:traffic-yellow={status === 'yellow'}
				class:traffic-inactive={status !== 'yellow'}
				data-testid="traffic-light-yellow"
			></div>
			<div
				class="traffic-indicator"
				class:traffic-green={status === 'green'}
				class:traffic-inactive={status !== 'green'}
				data-testid="traffic-light-green"
			></div>
		</div>
		{#if showLabel}
			<span class="text-sm font-medium text-primary">{trafficLightLabels[status]}</span>
		{/if}
	</div>
{:else}
	<span class="text-sm text-secondary">-</span>
{/if}
