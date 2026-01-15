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
</script>

<div class="space-y-3">
	<div class="flex items-center gap-4">
		<div class="text-center">
			<div 
				class="text-3xl font-bold"
				class:score-high={score !== null && score >= 75}
				class:score-medium={score !== null && score >= 50 && score < 75}
				class:score-low={score !== null && score < 50}
				class:text-secondary={score === null}
			>
				{score !== null ? score : '-'}
			</div>
			<div class="text-xs text-secondary">von 100</div>
		</div>
		<TrafficLightComponent status={trafficLight} />
	</div>

	{#if showReasons && reasons.length > 0}
		<div class="mt-4 space-y-2">
			<h4 class="text-sm font-medium text-primary">Bewertungsgründe:</h4>
			<ul class="space-y-1">
				{#each reasons as reason}
					<li class="text-sm text-secondary flex items-start gap-2">
						<span class="text-secondary mt-0.5">•</span>
						<span>{reason}</span>
					</li>
				{/each}
			</ul>
		</div>
	{/if}
</div>
