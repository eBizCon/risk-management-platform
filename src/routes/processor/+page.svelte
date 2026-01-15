<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import ApplicationTable from '$lib/components/ApplicationTable.svelte';
	import { Filter, FileText, CheckCircle, XCircle, Clock } from 'lucide-svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

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
		goto(url.toString());
	}

	function handleView(id: number) {
		goto(`/processor/${id}`);
	}
</script>

<svelte:head>
	<title>Anträge bearbeiten - Risikomanagement</title>
</svelte:head>

<div class="space-y-6">
	<div>
		<h1 class="text-2xl font-bold text-gray-900">Anträge bearbeiten</h1>
		<p class="text-gray-600 mt-1">Übersicht aller eingereichten Kreditanträge</p>
	</div>

	<div class="grid grid-cols-1 sm:grid-cols-4 gap-4">
		<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
			<div class="flex items-center gap-3">
				<div class="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center">
					<FileText class="w-5 h-5 text-gray-600" />
				</div>
				<div>
					<div class="text-2xl font-bold text-gray-900">{data.stats.total}</div>
					<div class="text-sm text-gray-500">Gesamt</div>
				</div>
			</div>
		</div>
		<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
			<div class="flex items-center gap-3">
				<div class="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
					<Clock class="w-5 h-5 text-blue-600" />
				</div>
				<div>
					<div class="text-2xl font-bold text-blue-600">{data.stats.submitted}</div>
					<div class="text-sm text-gray-500">Offen</div>
				</div>
			</div>
		</div>
		<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
			<div class="flex items-center gap-3">
				<div class="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
					<CheckCircle class="w-5 h-5 text-green-600" />
				</div>
				<div>
					<div class="text-2xl font-bold text-green-600">{data.stats.approved}</div>
					<div class="text-sm text-gray-500">Genehmigt</div>
				</div>
			</div>
		</div>
		<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
			<div class="flex items-center gap-3">
				<div class="w-10 h-10 bg-red-100 rounded-lg flex items-center justify-center">
					<XCircle class="w-5 h-5 text-red-600" />
				</div>
				<div>
					<div class="text-2xl font-bold text-red-600">{data.stats.rejected}</div>
					<div class="text-sm text-gray-500">Abgelehnt</div>
				</div>
			</div>
		</div>
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
			isApplicantView={false}
			onView={handleView}
		/>
	</div>
</div>
