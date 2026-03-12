<script lang="ts">
	import ApplicationForm from '$lib/components/ApplicationForm.svelte';
	import RoleGuard from '$lib/components/RoleGuard.svelte';
	import { ArrowLeft } from 'lucide-svelte';
	import type { PageData, ActionData } from './$types';

	let { data, form }: { data: PageData; form: ActionData } = $props();
</script>

<svelte:head>
	<title>Antrag bearbeiten - Risikomanagement</title>
</svelte:head>

<RoleGuard requiredRole="applicant">
<div class="space-y-6">
	<div class="flex flex-col sm:flex-row sm:items-center gap-3">
		<a
			href="/applications/{data.application.id}"
			class="back-link inline-flex items-center w-full sm:w-auto"
		>
			<ArrowLeft class="w-5 h-5 mr-1" />
			Zurück
		</a>
	</div>

	<div class="card p-6">
		<h1 class="text-2xl font-bold text-primary mb-6">Antrag #{data.application.id} bearbeiten</h1>
		
		<ApplicationForm application={data.application} errors={form?.errors} />
	</div>
</div>
</RoleGuard>
