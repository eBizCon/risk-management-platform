<script lang="ts">
	interface Props {
		label: string;
		count: number;
		color: string;
		bgColor: string;
		borderColor: string;
		testId: string;
	}

	let { label, count, color, bgColor, borderColor, testId }: Props = $props();
</script>

<div
	class="rounded-lg p-5 border"
	style="background-color: {bgColor}; border-color: {borderColor};"
	data-testid={testId}
>
	<p class="text-sm font-medium text-text-muted">{label}</p>
	<p class="text-2xl font-bold mt-1" style="color: {color};">{count}</p>
</div>
