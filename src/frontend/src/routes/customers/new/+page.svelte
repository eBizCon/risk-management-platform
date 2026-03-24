<script lang="ts">
	import CustomerForm from '$lib/components/CustomerForm.svelte';
	import RoleGuard from '$lib/components/RoleGuard.svelte';
	import { ArrowLeft } from 'lucide-svelte';
</script>

<svelte:head>
	<title>Neuer Kunde - Risikomanagement</title>
</svelte:head>

<RoleGuard requiredRole="applicant">
	<div class="space-y-6">
		<div class="flex items-center gap-4">
			<a href="/customers" class="back-link inline-flex items-center">
				<ArrowLeft class="w-5 h-5 mr-1" />
				Zurück
			</a>
			<h1 class="text-2xl font-bold text-primary" data-testid="customer-new-title">Neuer Kunde</h1>
		</div>

		<CustomerForm />
	</div>
</RoleGuard>
