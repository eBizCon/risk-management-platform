<script lang="ts">
	import type { TrafficLight } from '$lib/types';
	import TrafficLightComponent from './TrafficLight.svelte';

	interface Props {
		score: number | null;
		trafficLight: TrafficLight | null;
		reasons?: string[];
		showReasons?: boolean;
	}

	let { score, trafficLight, reasons = [], showReasons = false }: Props = $props();

	const scoreColorClass = $derived(
		score === null
			? 'text-gray-400'
			: score >= 75
				? 'text-green-600'
				: score >= 50
					? 'text-yellow-600'
					: 'text-red-600'
	);
</script>

<div class="space-y-3">
	<div class="flex items-center gap-4">
		<div class="text-center">
			<div class="text-3xl font-bold {scoreColorClass}">
				{score !== null ? score : '-'}
			</div>
			<div class="text-xs text-gray-500">von 100</div>
		</div>
		<TrafficLightComponent status={trafficLight} />
	</div>

	{#if showReasons && reasons.length > 0}
		<div class="mt-4 space-y-2">
			<h4 class="text-sm font-medium text-gray-700">Bewertungsgründe:</h4>
			<ul class="space-y-1">
				{#each reasons as reason}
					<li class="text-sm text-gray-600 flex items-start gap-2">
						<span class="text-gray-400 mt-0.5">•</span>
						<span>{reason}</span>
					</li>
				{/each}
			</ul>
		</div>
	{/if}
</div>
