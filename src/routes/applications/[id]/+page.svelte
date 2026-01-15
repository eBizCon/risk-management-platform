<script lang="ts">
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import StatusBadge from '$lib/components/StatusBadge.svelte';
	import ScoreDisplay from '$lib/components/ScoreDisplay.svelte';
	import { employmentStatusLabels } from '$lib/types';
	import { ArrowLeft, Edit, Send } from 'lucide-svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	const app = $derived(data.application);
	const reasons = $derived(app.scoringReasons ? JSON.parse(app.scoringReasons) : []);
	const showSubmittedMessage = $derived($page.url.searchParams.get('submitted') === 'true');

	function formatDate(dateString: string | null): string {
		if (!dateString) return '-';
		return new Date(dateString).toLocaleDateString('de-DE', {
			day: '2-digit',
			month: '2-digit',
			year: 'numeric',
			hour: '2-digit',
			minute: '2-digit'
		});
	}

	function formatCurrency(value: number): string {
		return new Intl.NumberFormat('de-DE', {
			style: 'currency',
			currency: 'EUR'
		}).format(value);
	}

	async function handleSubmit() {
		if (confirm('Möchten Sie diesen Antrag wirklich einreichen? Nach der Einreichung ist keine Bearbeitung mehr möglich.')) {
			const response = await fetch(`/api/applications/${app.id}/submit`, {
				method: 'POST'
			});
			if (response.ok) {
				window.location.reload();
			}
		}
	}
</script>

<svelte:head>
	<title>Antrag #{app.id} - Risikomanagement</title>
</svelte:head>

<div class="space-y-6">
	<div class="flex items-center justify-between">
		<div class="flex items-center gap-4">
			<a
				href="/applications"
				class="inline-flex items-center text-gray-600 hover:text-gray-900"
			>
				<ArrowLeft class="w-5 h-5 mr-1" />
				Zurück
			</a>
			<h1 class="text-2xl font-bold text-gray-900">Antrag #{app.id}</h1>
			<StatusBadge status={app.status} />
		</div>
		{#if app.status === 'draft'}
			<div class="flex gap-3">
				<a
					href="/applications/{app.id}/edit"
					class="inline-flex items-center px-4 py-2 bg-white text-gray-700 font-medium rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
				>
					<Edit class="w-4 h-4 mr-2" />
					Bearbeiten
				</a>
				<button
					onclick={handleSubmit}
					class="inline-flex items-center px-4 py-2 bg-indigo-600 text-white font-medium rounded-lg hover:bg-indigo-700 transition-colors"
				>
					<Send class="w-4 h-4 mr-2" />
					Einreichen
				</button>
			</div>
		{/if}
	</div>

	{#if showSubmittedMessage}
		<div class="bg-green-50 border border-green-200 rounded-lg p-4">
			<p class="text-green-800 font-medium">Ihr Antrag wurde erfolgreich eingereicht und wird nun geprüft.</p>
		</div>
	{/if}

	<div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
		<div class="lg:col-span-2 space-y-6">
			<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
				<h2 class="text-lg font-semibold text-gray-900 mb-4">Persönliche Daten</h2>
				<dl class="grid grid-cols-1 sm:grid-cols-2 gap-4">
					<div>
						<dt class="text-sm font-medium text-gray-500">Name</dt>
						<dd class="mt-1 text-sm text-gray-900">{app.name}</dd>
					</div>
					<div>
						<dt class="text-sm font-medium text-gray-500">Beschäftigungsstatus</dt>
						<dd class="mt-1 text-sm text-gray-900">{employmentStatusLabels[app.employmentStatus]}</dd>
					</div>
					<div>
						<dt class="text-sm font-medium text-gray-500">Zahlungsverzug in der Vergangenheit</dt>
						<dd class="mt-1 text-sm text-gray-900">{app.hasPaymentDefault ? 'Ja' : 'Nein'}</dd>
					</div>
				</dl>
			</div>

			<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
				<h2 class="text-lg font-semibold text-gray-900 mb-4">Finanzielle Details</h2>
				<dl class="grid grid-cols-1 sm:grid-cols-3 gap-4">
					<div>
						<dt class="text-sm font-medium text-gray-500">Monatliches Einkommen</dt>
						<dd class="mt-1 text-sm text-gray-900">{formatCurrency(app.income)}</dd>
					</div>
					<div>
						<dt class="text-sm font-medium text-gray-500">Monatliche Fixkosten</dt>
						<dd class="mt-1 text-sm text-gray-900">{formatCurrency(app.fixedCosts)}</dd>
					</div>
					<div>
						<dt class="text-sm font-medium text-gray-500">Verfügbares Einkommen</dt>
						<dd class="mt-1 text-sm text-gray-900 font-medium">{formatCurrency(app.income - app.fixedCosts)}</dd>
					</div>
					<div class="sm:col-span-3">
						<dt class="text-sm font-medium text-gray-500">Gewünschte Rate</dt>
						<dd class="mt-1 text-lg text-gray-900 font-semibold">{formatCurrency(app.desiredRate)}</dd>
					</div>
				</dl>
			</div>

			<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
				<h2 class="text-lg font-semibold text-gray-900 mb-4">Zeitstempel</h2>
				<dl class="grid grid-cols-1 sm:grid-cols-3 gap-4">
					<div>
						<dt class="text-sm font-medium text-gray-500">Erstellt am</dt>
						<dd class="mt-1 text-sm text-gray-900">{formatDate(app.createdAt)}</dd>
					</div>
					<div>
						<dt class="text-sm font-medium text-gray-500">Eingereicht am</dt>
						<dd class="mt-1 text-sm text-gray-900">{formatDate(app.submittedAt)}</dd>
					</div>
					<div>
						<dt class="text-sm font-medium text-gray-500">Bearbeitet am</dt>
						<dd class="mt-1 text-sm text-gray-900">{formatDate(app.processedAt)}</dd>
					</div>
				</dl>
			</div>

			{#if app.processorComment}
				<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
					<h2 class="text-lg font-semibold text-gray-900 mb-4">Kommentar des Bearbeiters</h2>
					<p class="text-gray-700">{app.processorComment}</p>
				</div>
			{/if}
		</div>

		<div class="space-y-6">
			<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
				<h2 class="text-lg font-semibold text-gray-900 mb-4">Bewertung</h2>
				<ScoreDisplay
					score={app.score}
					trafficLight={app.trafficLight}
					{reasons}
					showReasons={true}
				/>
			</div>
		</div>
	</div>
</div>
