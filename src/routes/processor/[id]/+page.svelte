<script lang="ts">
	import { page } from '$app/stores';
	import { enhance } from '$app/forms';
	import StatusBadge from '$lib/components/StatusBadge.svelte';
	import ScoreDisplay from '$lib/components/ScoreDisplay.svelte';
	import RoleGuard from '$lib/components/RoleGuard.svelte';
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

<RoleGuard requiredRole="processor" redirectTo="/applications">
<div class="space-y-6">
	<div class="flex items-center gap-4">
		<a
			href="/processor"
			class="back-link inline-flex items-center"
		>
			<ArrowLeft class="w-5 h-5 mr-1" />
			Zurück
		</a>
		<h1 class="text-2xl font-bold text-primary">Antrag #{app.id} prüfen</h1>
		<StatusBadge status={app.status} />
	</div>

	{#if showProcessedMessage}
		<div class="success-message rounded-lg p-4">
			<p class="font-medium">Der Antrag wurde erfolgreich bearbeitet.</p>
		</div>
	{/if}

	<div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
		<div class="lg:col-span-2 space-y-6">
			<div class="card p-6">
				<h2 class="text-lg font-semibold text-primary mb-4">Antragsteller</h2>
				<dl class="grid grid-cols-1 sm:grid-cols-2 gap-4">
					<div>
						<dt class="dl-label">Name</dt>
						<dd class="mt-1 dl-value">{app.name}</dd>
					</div>
					<div>
						<dt class="dl-label">Beschäftigungsstatus</dt>
						<dd class="mt-1 dl-value">{employmentStatusLabels[app.employmentStatus]}</dd>
					</div>
					<div>
						<dt class="dl-label">Zahlungsverzug in der Vergangenheit</dt>
						<dd class="mt-1 dl-value">{app.hasPaymentDefault ? 'Ja' : 'Nein'}</dd>
					</div>
				</dl>
			</div>

			<div class="card p-6">
				<h2 class="text-lg font-semibold text-primary mb-4">Finanzielle Situation</h2>
				<dl class="grid grid-cols-1 sm:grid-cols-2 gap-4">
					<div>
						<dt class="dl-label">Monatliches Einkommen</dt>
						<dd class="mt-1 dl-value">{formatCurrency(app.income)}</dd>
					</div>
					<div>
						<dt class="dl-label">Monatliche Fixkosten</dt>
						<dd class="mt-1 dl-value">{formatCurrency(app.fixedCosts)}</dd>
					</div>
					<div>
						<dt class="dl-label">Verfügbares Einkommen</dt>
						<dd class="mt-1 dl-value font-medium">{formatCurrency(app.income - app.fixedCosts)}</dd>
					</div>
					<div>
						<dt class="dl-label">Gewünschte Rate</dt>
						<dd class="mt-1 text-lg text-primary font-semibold">{formatCurrency(app.desiredRate)}</dd>
					</div>
				</dl>
				<div class="mt-4 pt-4 border-t border-default">
					<div class="text-sm text-secondary">
						Rate als Anteil des verfügbaren Einkommens: 
						<span class="font-medium text-primary">
							{((app.desiredRate / (app.income - app.fixedCosts)) * 100).toFixed(1)}%
						</span>
					</div>
				</div>
			</div>

			<div class="card p-6">
				<h2 class="text-lg font-semibold text-primary mb-4">Zeitverlauf</h2>
				<dl class="grid grid-cols-1 sm:grid-cols-3 gap-4">
					<div>
						<dt class="dl-label">Erstellt am</dt>
						<dd class="mt-1 dl-value">{formatDate(app.createdAt)}</dd>
					</div>
					<div>
						<dt class="dl-label">Eingereicht am</dt>
						<dd class="mt-1 dl-value">{formatDate(app.submittedAt)}</dd>
					</div>
					<div>
						<dt class="dl-label">Bearbeitet am</dt>
						<dd class="mt-1 dl-value">{formatDate(app.processedAt)}</dd>
					</div>
				</dl>
			</div>

			{#if app.processorComment}
				<div class="card p-6">
					<h2 class="text-lg font-semibold text-primary mb-4">Bearbeiterkommentar</h2>
					<p class="text-secondary">{app.processorComment}</p>
				</div>
			{/if}

			{#if canProcess}
				<div class="card p-6">
					<h2 class="text-lg font-semibold text-primary mb-4">Entscheidung treffen</h2>
					
					<form method="POST" use:enhance class="space-y-4">
						<div>
							<label class="form-label block mb-2">Entscheidung</label>
							<div class="flex gap-4">
								<label class="flex items-center gap-2 cursor-pointer">
									<input
										type="radio"
										name="decision"
										value="approved"
										bind:group={selectedDecision}
										class="h-4 w-4 border-default radio-success"
									/>
									<CheckCircle class="w-5 h-5 stat-value-success" />
									<span class="text-sm font-medium text-primary">Genehmigen</span>
								</label>
								<label class="flex items-center gap-2 cursor-pointer">
									<input
										type="radio"
										name="decision"
										value="rejected"
										bind:group={selectedDecision}
										class="h-4 w-4 border-default radio-danger"
									/>
									<XCircle class="w-5 h-5 stat-value-danger" />
									<span class="text-sm font-medium text-primary">Ablehnen</span>
								</label>
							</div>
							{#if form?.errors?.decision}
								<p class="mt-1 error-text">{form.errors.decision[0]}</p>
							{/if}
						</div>

						<div>
							<label for="comment" class="form-label block">
								Kommentar {selectedDecision === 'rejected' ? '(Pflichtfeld bei Ablehnung)' : '(optional)'}
							</label>
							<textarea
								id="comment"
								name="comment"
								rows="3"
								bind:value={comment}
								class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
								placeholder="Begründung oder Hinweise..."
							></textarea>
							{#if form?.errors?.comment}
								<p class="mt-1 error-text">{form.errors.comment[0]}</p>
							{/if}
						</div>

						<div class="flex justify-end">
							<button
								type="submit"
								disabled={!selectedDecision}
								class="decision-btn px-4 py-2 text-sm font-medium text-white rounded-md shadow-sm disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
								class:btn-approve={selectedDecision === 'approved'}
								class:btn-reject={selectedDecision === 'rejected'}
								class:btn-neutral={!selectedDecision}
							>
								{selectedDecision === 'approved' ? 'Antrag genehmigen' : selectedDecision === 'rejected' ? 'Antrag ablehnen' : 'Entscheidung treffen'}
							</button>
						</div>
					</form>
				</div>
			{/if}
		</div>

		<div class="space-y-6">
			<div class="card p-6">
				<h2 class="text-lg font-semibold text-primary mb-4">Automatische Bewertung</h2>
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
</RoleGuard>
