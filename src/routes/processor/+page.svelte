<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import ApplicationTable from '$lib/components/ApplicationTable.svelte';
	import Pagination from '$lib/components/Pagination.svelte';
	import RoleGuard from '$lib/components/RoleGuard.svelte';
	import { Filter, FileText, CheckCircle, XCircle, Clock } from 'lucide-svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
	const pagination = $derived(data.pagination ?? { page: 1, totalPages: 1, totalItems: data.applications.length ?? 0 });
	const exportHref = $derived.by(() => {
		const url = new URL($page.url);
		url.pathname = '/api/processor/applications.csv';
		url.searchParams.delete('page');
		return url.toString();
	});

	const statusOptions = [
		{ value: '', label: 'Alle Status' },
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
		goto(`/processor/${id}`);
	}

	function handlePageChange(newPage: number) {
		const url = new URL($page.url);
		url.searchParams.set('page', newPage.toString());
		goto(url.toString());
	}
</script>

<svelte:head>
	<title>Anträge bearbeiten - Risikomanagement</title>
</svelte:head>

<RoleGuard requiredRole="processor">
<div class="space-y-6">
	<div>
		<h1 class="text-2xl font-bold text-primary">Anträge bearbeiten</h1>
		<p class="text-secondary mt-1">Übersicht aller eingereichten Kreditanträge</p>
	</div>

	<div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
		<div class="card p-4">
			<div class="flex items-center gap-3">
				<div class="w-10 h-10 stat-icon-neutral rounded-lg flex items-center justify-center">
					<FileText class="w-5 h-5" />
				</div>
				<div>
					<div class="text-2xl font-bold text-primary">{data.stats.total}</div>
					<div class="text-sm text-secondary">Gesamt</div>
				</div>
			</div>
		</div>
		<div class="card p-4">
			<div class="flex items-center gap-3">
				<div class="w-10 h-10 stat-icon-info rounded-lg flex items-center justify-center">
					<Clock class="w-5 h-5" />
				</div>
				<div>
					<div class="text-2xl font-bold stat-value-info">{data.stats.submitted}</div>
					<div class="text-sm text-secondary">Offen</div>
				</div>
			</div>
		</div>
		<div class="card p-4">
			<div class="flex items-center gap-3">
				<div class="w-10 h-10 stat-icon-success rounded-lg flex items-center justify-center">
					<CheckCircle class="w-5 h-5" />
				</div>
				<div>
					<div class="text-2xl font-bold stat-value-success">{data.stats.approved}</div>
					<div class="text-sm text-secondary">Genehmigt</div>
				</div>
			</div>
		</div>
		<div class="card p-4">
			<div class="flex items-center gap-3">
				<div class="w-10 h-10 stat-icon-danger rounded-lg flex items-center justify-center">
					<XCircle class="w-5 h-5" />
				</div>
				<div>
					<div class="text-2xl font-bold stat-value-danger">{data.stats.rejected}</div>
					<div class="text-sm text-secondary">Abgelehnt</div>
				</div>
			</div>
		</div>
	</div>

	<div class="card">
		<div class="px-4 py-4 border-b border-default">
			<div class="flex flex-col sm:flex-row sm:items-center gap-3 sm:gap-4">
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
					{pagination.totalItems} Antrag/Anträge gefunden
				</span>
				<a
					href={exportHref}
					class="btn-secondary inline-flex items-center gap-2"
					data-testid="processor-applications-export-csv-button"
				>
					<FileText class="w-4 h-4" />
					CSV herunterladen
				</a>
			</div>
		</div>

		<ApplicationTable
			applications={data.applications}
			isApplicantView={false}
			onView={handleView}
		/>
		<div class="p-4 border-t border-default flex justify-center">
			<Pagination
				page={pagination.page}
				totalPages={pagination.totalPages}
				onPageChange={handlePageChange}
			/>
		</div>
	</div>
</div>
</RoleGuard>
