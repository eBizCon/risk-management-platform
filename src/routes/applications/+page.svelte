<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import ApplicationTable from '$lib/components/ApplicationTable.svelte';
	import Pagination from '$lib/components/Pagination.svelte';
	import RoleGuard from '$lib/components/RoleGuard.svelte';
	import { Plus, Filter } from 'lucide-svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
	const pagination = $derived(data.pagination ?? { page: 1, totalPages: 1, totalItems: data.applications.length ?? 0 });

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
		url.searchParams.delete('page');
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

	function handlePageChange(newPage: number) {
		const url = new URL($page.url);
		url.searchParams.set('page', newPage.toString());
		goto(url.toString());
	}
</script>

<svelte:head>
	<title>Meine Anträge - Risikomanagement</title>
</svelte:head>

<RoleGuard requiredRole="applicant">
<div class="space-y-6">
	<div class="flex flex-col sm:flex-row sm:justify-between sm:items-center gap-4">
		<div>
			<h1 class="text-2xl font-bold text-primary">Meine Anträge</h1>
			<p class="text-secondary mt-1">Übersicht über alle Ihre Kreditanträge</p>
		</div>
		<a
			href="/applications/new"
			class="btn-primary inline-flex items-center px-4 py-2 w-full sm:w-auto"
		>
			<Plus class="w-5 h-5 mr-2" />
			Neuer Antrag
		</a>
	</div>

	<div class="card">
		<div class="px-4 py-4 border-b border-default">
			<div class="flex flex-col sm:flex-row sm:items-center gap-3 sm:gap-4">
				<div class="flex items-center gap-2">
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
				</div>
					<span class="text-sm text-secondary">
					{pagination.totalItems} Antrag/Anträge gefunden
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
		<div class="p-4 border-t border-default flex justify-center">
			<Pagination
				page={pagination.page}
				totalPages={pagination.totalPages}
				onPageChange={handlePageChange}
				testIdPrefix="applicant"
			/>
		</div>
	</div>
</div>
</RoleGuard>
