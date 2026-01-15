<script lang="ts">
	import ApplicationForm from '$lib/components/ApplicationForm.svelte';
	import RoleGuard from '$lib/components/RoleGuard.svelte';
	import { ArrowLeft } from 'lucide-svelte';
	import type { ActionData } from './$types';

	let { form }: { form: ActionData } = $props();
</script>

<svelte:head>
	<title>Neuer Antrag - Risikomanagement</title>
</svelte:head>

<RoleGuard requiredRole="applicant" redirectTo="/processor">
<div class="space-y-6">
	<div class="flex items-center gap-4">
		<a
			href="/applications"
			class="back-link inline-flex items-center"
		>
			<ArrowLeft class="w-5 h-5 mr-1" />
			Zurück
		</a>
	</div>

	<div class="card p-6">
		<h1 class="text-2xl font-bold text-primary mb-6">Neuen Kreditantrag erstellen</h1>
		
		<ApplicationForm errors={form?.errors} />
	</div>
</div>
</RoleGuard>
