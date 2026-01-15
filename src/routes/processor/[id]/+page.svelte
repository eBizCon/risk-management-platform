<script lang="ts">
	import { page } from '$app/stores';
	import { enhance } from '$app/forms';
	import StatusBadge from '$lib/components/StatusBadge.svelte';
	import ScoreDisplay from '$lib/components/ScoreDisplay.svelte';
	import { employmentStatusLabels } from '$lib/types';
	import { ArrowLeft, CheckCircle, XCircle } from 'lucide-svelte';
	import type { PageData, ActionData } from './$types';

	let { data, form }: { data: PageData; form: ActionData } = $props();

	const app = $derived(data.application);
	const reasons = $derived(app.scoringReasons ? JSON.parse(app.scoringReasons) : []);
	const showProcessedMessage = $derived($page.url.searchParams.get('processed') === 'true');
	const canProcess = $derived(app.status === 'submitted');

	let selectedDecision = $state<'approved' | 'rejected' | ''>('');
	let comment = $state('');

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
</script>

<svelte:head>
	<title>Antrag #{app.id} prüfen - Risikomanagement</title>
</svelte:head>

<div class="space-y-6">
	<div class="flex items-center gap-4">
		<a
			href="/processor"
			class="inline-flex items-center text-gray-600 hover:text-gray-900"
		>
			<ArrowLeft class="w-5 h-5 mr-1" />
			Zurück
		</a>
		<h1 class="text-2xl font-bold text-gray-900">Antrag #{app.id} prüfen</h1>
		<StatusBadge status={app.status} />
	</div>

	{#if showProcessedMessage}
		<div class="bg-green-50 border border-green-200 rounded-lg p-4">
			<p class="text-green-800 font-medium">Der Antrag wurde erfolgreich bearbeitet.</p>
		</div>
	{/if}

	<div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
		<div class="lg:col-span-2 space-y-6">
			<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
				<h2 class="text-lg font-semibold text-gray-900 mb-4">Antragsteller</h2>
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
				<h2 class="text-lg font-semibold text-gray-900 mb-4">Finanzielle Situation</h2>
				<dl class="grid grid-cols-1 sm:grid-cols-2 gap-4">
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
					<div>
						<dt class="text-sm font-medium text-gray-500">Gewünschte Rate</dt>
						<dd class="mt-1 text-lg text-gray-900 font-semibold">{formatCurrency(app.desiredRate)}</dd>
					</div>
				</dl>
				<div class="mt-4 pt-4 border-t border-gray-200">
					<div class="text-sm text-gray-500">
						Rate als Anteil des verfügbaren Einkommens: 
						<span class="font-medium text-gray-900">
							{((app.desiredRate / (app.income - app.fixedCosts)) * 100).toFixed(1)}%
						</span>
					</div>
				</div>
			</div>

			<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
				<h2 class="text-lg font-semibold text-gray-900 mb-4">Zeitverlauf</h2>
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
					<h2 class="text-lg font-semibold text-gray-900 mb-4">Bearbeiterkommentar</h2>
					<p class="text-gray-700">{app.processorComment}</p>
				</div>
			{/if}

			{#if canProcess}
				<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
					<h2 class="text-lg font-semibold text-gray-900 mb-4">Entscheidung treffen</h2>
					
					<form method="POST" use:enhance class="space-y-4">
						<div>
							<label class="block text-sm font-medium text-gray-700 mb-2">Entscheidung</label>
							<div class="flex gap-4">
								<label class="flex items-center gap-2 cursor-pointer">
									<input
										type="radio"
										name="decision"
										value="approved"
										bind:group={selectedDecision}
										class="h-4 w-4 border-gray-300 text-green-600 focus:ring-green-500"
									/>
									<CheckCircle class="w-5 h-5 text-green-600" />
									<span class="text-sm font-medium text-gray-700">Genehmigen</span>
								</label>
								<label class="flex items-center gap-2 cursor-pointer">
									<input
										type="radio"
										name="decision"
										value="rejected"
										bind:group={selectedDecision}
										class="h-4 w-4 border-gray-300 text-red-600 focus:ring-red-500"
									/>
									<XCircle class="w-5 h-5 text-red-600" />
									<span class="text-sm font-medium text-gray-700">Ablehnen</span>
								</label>
							</div>
							{#if form?.errors?.decision}
								<p class="mt-1 text-sm text-red-600">{form.errors.decision[0]}</p>
							{/if}
						</div>

						<div>
							<label for="comment" class="block text-sm font-medium text-gray-700">
								Kommentar {selectedDecision === 'rejected' ? '(Pflichtfeld bei Ablehnung)' : '(optional)'}
							</label>
							<textarea
								id="comment"
								name="comment"
								rows="3"
								bind:value={comment}
								class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
								placeholder="Begründung oder Hinweise..."
							></textarea>
							{#if form?.errors?.comment}
								<p class="mt-1 text-sm text-red-600">{form.errors.comment[0]}</p>
							{/if}
						</div>

						<div class="flex justify-end">
							<button
								type="submit"
								disabled={!selectedDecision}
								class="px-4 py-2 text-sm font-medium text-white rounded-md shadow-sm disabled:opacity-50 disabled:cursor-not-allowed transition-colors {selectedDecision === 'approved' ? 'bg-green-600 hover:bg-green-700' : selectedDecision === 'rejected' ? 'bg-red-600 hover:bg-red-700' : 'bg-gray-400'}"
							>
								{selectedDecision === 'approved' ? 'Antrag genehmigen' : selectedDecision === 'rejected' ? 'Antrag ablehnen' : 'Entscheidung treffen'}
							</button>
						</div>
					</form>
				</div>
			{/if}
		</div>

		<div class="space-y-6">
			<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
				<h2 class="text-lg font-semibold text-gray-900 mb-4">Automatische Bewertung</h2>
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
