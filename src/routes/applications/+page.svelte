<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import ApplicationTable from '$lib/components/ApplicationTable.svelte';
	import RoleGuard from '$lib/components/RoleGuard.svelte';
	import { Plus, Filter } from 'lucide-svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	const statusOptions = [
		{ value: '', label: 'Alle Status' },
		{ value: 'draft', label: 'Entwürfe' },
		{ value: 'submitted', label: 'Eingereicht' },
		{ value: 'approved', label: 'Genehmigt' },
		{ value: 'rejected', label: 'Abgelehnt' }
	];

	function handleFilterChange(event: Event) {
		const select = event.target as HTMLSelectElement;
		const url = new URL($page.url);
		if (select.value) {
			url.searchParams.set('status', select.value);
		} else {
			url.searchParams.delete('status');
		}
		goto(url.toString());
	}

	function handleView(id: number) {
		goto(`/applications/${id}`);
	}

	function handleEdit(id: number) {
		goto(`/applications/${id}/edit`);
	}

	async function handleDelete(id: number) {
		if (confirm('Möchten Sie diesen Entwurf wirklich löschen?')) {
			const response = await fetch(`/api/applications/${id}`, {
				method: 'DELETE'
			});
			if (response.ok) {
				window.location.reload();
			}
		}
	}
</script>

<svelte:head>
	<title>Meine Anträge - Risikomanagement</title>
</svelte:head>

<RoleGuard requiredRole="applicant" redirectTo="/processor">
<div class="space-y-6">
	<div class="flex justify-between items-center">
		<div>
			<h1 class="text-2xl font-bold text-primary">Meine Anträge</h1>
			<p class="text-secondary mt-1">Übersicht über alle Ihre Kreditanträge</p>
		</div>
		<a
			href="/applications/new"
			class="btn-primary inline-flex items-center px-4 py-2"
		>
			<Plus class="w-5 h-5 mr-2" />
			Neuer Antrag
		</a>
	</div>

	<div class="card">
		<div class="p-4 border-b border-default">
			<div class="flex items-center gap-4">
				<Filter class="w-5 h-5 text-secondary" />
				<select
					onchange={handleFilterChange}
					value={data.statusFilter || ''}
					class="rounded-md border-default shadow-sm sm:text-sm"
				>
					{#each statusOptions as option}
						<option value={option.value}>{option.label}</option>
					{/each}
				</select>
				<span class="text-sm text-secondary">
					{data.applications.length} Antrag/Anträge gefunden
				</span>
			</div>
		</div>

		<ApplicationTable
			applications={data.applications}
			isApplicantView={true}
			onView={handleView}
			onEdit={handleEdit}
			onDelete={handleDelete}
		/>
	</div>
</div>
</RoleGuard>
