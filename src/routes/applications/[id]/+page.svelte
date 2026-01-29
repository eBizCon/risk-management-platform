<script lang="ts">
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import ConfirmDialog from '$lib/components/ConfirmDialog.svelte';
	import StatusBadge from '$lib/components/StatusBadge.svelte';
	import ScoreDisplay from '$lib/components/ScoreDisplay.svelte';
	import RoleGuard from '$lib/components/RoleGuard.svelte';
	import { employmentStatusLabels } from '$lib/types';
	import { ArrowLeft, Edit, Send } from 'lucide-svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	const app = $derived(data.application);
	const reasons = $derived(app.scoringReasons ? JSON.parse(app.scoringReasons) : []);
	const showSubmittedMessage = $derived($page.url.searchParams.get('submitted') === 'true');
	let showConfirmDialog = $state(false);

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

	function handleOpenConfirm() {
		showConfirmDialog = true;
	}

	async function handleConfirmSubmit() {
		showConfirmDialog = false;
		const response = await fetch(`/api/applications/${app.id}/submit`, {
			method: 'POST'
		});
		if (response.ok) {
			window.location.reload();
		}
	}

	function handleCancelSubmit() {
		showConfirmDialog = false;
	}
</script>

<svelte:head>
	<title>Antrag #{app.id} - Risikomanagement</title>
</svelte:head>

<RoleGuard requiredRole="applicant">
<div class="space-y-6">
	<div class="flex items-center justify-between">
		<div class="flex items-center gap-4">
			<a
				href="/applications"
				class="back-link inline-flex items-center"
			>
				<ArrowLeft class="w-5 h-5 mr-1" />
				Zurück
			</a>
			<h1 class="text-2xl font-bold text-primary" data-testid="application-title">Antrag #{app.id}</h1>
			<div data-testid="status-badge-container">
				<StatusBadge status={app.status} />
			</div>
		</div>
		{#if app.status === 'draft'}
			<div class="flex gap-3">
				<a
					href="/applications/{app.id}/edit"
					class="btn-secondary inline-flex items-center px-4 py-2"
					data-testid="edit-application"
				>
					<Edit class="w-4 h-4 mr-2" />
					Bearbeiten
				</a>
				<button
					data-testid="submit-application"
					onclick={handleOpenConfirm}
					class="btn-primary inline-flex items-center px-4 py-2"
				>
					<Send class="w-4 h-4 mr-2" />
					Einreichen
				</button>
			</div>
		{/if}
	</div>

	{#if showSubmittedMessage}
		<div class="success-message rounded-lg p-4">
			<p class="font-medium">Ihr Antrag wurde erfolgreich eingereicht und wird nun geprüft.</p>
		</div>
	{/if}

	<div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
		<div class="lg:col-span-2 space-y-6">
			<div class="card p-6">
				<h2 class="text-lg font-semibold text-primary mb-4">Persönliche Daten</h2>
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
				<h2 class="text-lg font-semibold text-primary mb-4">Finanzielle Details</h2>
				<dl class="grid grid-cols-1 sm:grid-cols-3 gap-4">
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
					<div class="sm:col-span-3">
						<dt class="dl-label">Gewünschte Rate</dt>
						<dd class="mt-1 text-lg text-primary font-semibold">{formatCurrency(app.desiredRate)}</dd>
					</div>
				</dl>
			</div>

			<div class="card p-6">
				<h2 class="text-lg font-semibold text-primary mb-4">Zeitstempel</h2>
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
					<h2 class="text-lg font-semibold text-primary mb-4">Kommentar des Bearbeiters</h2>
					<p class="text-secondary">{app.processorComment}</p>
				</div>
			{/if}
		</div>

		<div class="space-y-6">
			<div class="card p-6">
				<h2 class="text-lg font-semibold text-primary mb-4" data-testid="scoring-heading">Bewertung</h2>
				<ScoreDisplay
					score={app.score}
					trafficLight={app.trafficLight}
					{reasons}
					showReasons={true}
				/>
			</div>
		</div>
	</div>

	<ConfirmDialog
		open={showConfirmDialog}
		message="Möchten Sie diesen Antrag wirklich einreichen? Nach der Einreichung ist keine Bearbeitung mehr möglich."
		confirmText="Antrag einreichen"
		cancelText="Abbrechen"
		onConfirm={handleConfirmSubmit}
		onCancel={handleCancelSubmit}
	/>
</div>
</RoleGuard>
