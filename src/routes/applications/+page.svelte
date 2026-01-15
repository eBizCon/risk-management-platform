<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import ApplicationTable from '$lib/components/ApplicationTable.svelte';
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

<div class="space-y-6">
	<div class="flex justify-between items-center">
		<div>
			<h1 class="text-2xl font-bold text-gray-900">Meine Anträge</h1>
			<p class="text-gray-600 mt-1">Übersicht über alle Ihre Kreditanträge</p>
		</div>
		<a
			href="/applications/new"
			class="inline-flex items-center px-4 py-2 bg-indigo-600 text-white font-medium rounded-lg hover:bg-indigo-700 transition-colors"
		>
			<Plus class="w-5 h-5 mr-2" />
			Neuer Antrag
		</a>
	</div>

	<div class="bg-white rounded-lg shadow-sm border border-gray-200">
		<div class="p-4 border-b border-gray-200">
			<div class="flex items-center gap-4">
				<Filter class="w-5 h-5 text-gray-400" />
				<select
					onchange={handleFilterChange}
					value={data.statusFilter || ''}
					class="rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
				>
					{#each statusOptions as option}
						<option value={option.value}>{option.label}</option>
					{/each}
				</select>
				<span class="text-sm text-gray-500">
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
