<script lang="ts">
	import ApplicationForm from '$lib/components/ApplicationForm.svelte';
	import { ArrowLeft } from 'lucide-svelte';
	import type { ActionData } from './$types';

	let { form }: { form: ActionData } = $props();
</script>

<svelte:head>
	<title>Neuer Antrag - Risikomanagement</title>
</svelte:head>

<div class="space-y-6">
	<div class="flex items-center gap-4">
		<a
			href="/applications"
			class="inline-flex items-center text-gray-600 hover:text-gray-900"
		>
			<ArrowLeft class="w-5 h-5 mr-1" />
			Zurück
		</a>
	</div>

	<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
		<h1 class="text-2xl font-bold text-gray-900 mb-6">Neuen Kreditantrag erstellen</h1>
		
		<ApplicationForm errors={form?.errors} />
	</div>
</div>
