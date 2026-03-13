<script lang="ts">
	import CustomerForm from '$lib/components/CustomerForm.svelte';
	import RoleGuard from '$lib/components/RoleGuard.svelte';
	import { ArrowLeft } from 'lucide-svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
</script>

<svelte:head>
	<title>Kunde bearbeiten - Risikomanagement</title>
</svelte:head>

<RoleGuard requiredRole="applicant">
	<div class="space-y-6">
		<div class="flex items-center gap-4">
			<a href="/customers/{data.customer.id}" class="back-link inline-flex items-center">
				<ArrowLeft class="w-5 h-5 mr-1" />
				Zurück
			</a>
			<h1 class="text-2xl font-bold text-primary" data-testid="customer-edit-title">
				Kunde bearbeiten
			</h1>
		</div>

		<CustomerForm customer={data.customer} />
	</div>
</RoleGuard>
