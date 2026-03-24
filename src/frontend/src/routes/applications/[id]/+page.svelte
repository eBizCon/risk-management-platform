<script lang="ts">
	import { page } from '$app/stores';
	import { goto, invalidateAll } from '$app/navigation';
	import ConfirmDialog from '$lib/components/ConfirmDialog.svelte';
	import StatusBadge from '$lib/components/StatusBadge.svelte';
	import ScoreDisplay from '$lib/components/ScoreDisplay.svelte';
	import RoleGuard from '$lib/components/RoleGuard.svelte';
	import { employmentStatusLabels } from '$lib/types';
	import type { Application } from '$lib/types';
	import { ArrowLeft, Edit, Send, Loader2, AlertTriangle, RefreshCw, Trash2 } from 'lucide-svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	let polledApplication = $state<Application | null>(null);
	const app = $derived(polledApplication ?? data.application);
	const inquiries = $derived(data.inquiries);
	const reasons = $derived(app.scoringReasons ? JSON.parse(app.scoringReasons) : []);
	const showSubmittedMessage = $derived($page.url.searchParams.get('submitted') === 'true');
	const showInquiryAnsweredMessage = $derived(
		$page.url.searchParams.get('answeredInquiry') === 'true'
	);
	const canAnswerInquiry = $derived(app.status === 'needs_information');
	const isProcessing = $derived(app.status === 'processing');
	const isFailed = $derived(app.status === 'failed');
	let showConfirmDialog = $state(false);
	let responseText = $state('');
	let answerInquiryErrors = $state<Record<string, string[]>>({});
	let isRetrying = $state(false);

	let pollTimer: ReturnType<typeof setInterval> | null = null;

	$effect(() => {
		if (isProcessing) {
			startPolling();
		} else {
			stopPolling();
		}
		return () => stopPolling();
	});

	function startPolling() {
		if (pollTimer) return;
		pollTimer = setInterval(async () => {
			try {
				const res = await fetch(`/api/applications/${app.id}`);
				if (res.ok) {
					const updated: Application = await res.json();
					polledApplication = updated;
					if (updated.status !== 'processing') {
						stopPolling();
					}
				}
			} catch {
				// ignore fetch errors during polling
			}
		}, 2000);
	}

	function stopPolling() {
		if (pollTimer) {
			clearInterval(pollTimer);
			pollTimer = null;
		}
	}

	async function handleRetry() {
		isRetrying = true;
		try {
			await fetch(`/api/applications/${app.id}`, { method: 'DELETE' });

			const res = await fetch('/api/applications?submit=true', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({
					customerId: app.customerId,
					income: app.income,
					fixedCosts: app.fixedCosts,
					desiredRate: app.desiredRate
				})
			});

			if (res.ok) {
				const result = await res.json();
				if (result.application?.id) {
					await goto(`/applications/${result.application.id}?processing=true`);
				}
			}
		} finally {
			isRetrying = false;
		}
	}

	async function handleDeleteFailed() {
		if (confirm('Möchten Sie diesen fehlgeschlagenen Antrag wirklich löschen?')) {
			const res = await fetch(`/api/applications/${app.id}`, { method: 'DELETE' });
			if (res.ok) {
				await goto('/applications');
			}
		}
	}

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

	async function handleAnswerInquiry(event: Event) {
		event.preventDefault();
		answerInquiryErrors = {};

		const res = await fetch(`/api/applications/${app.id}/inquiry/response`, {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ responseText })
		});

		if (!res.ok) {
			const result = await res.json();
			if (result.errors) {
				answerInquiryErrors = result.errors;
			}
			return;
		}

		await goto(`/applications/${app.id}?answeredInquiry=true`);
		await invalidateAll();
	}
</script>

<svelte:head>
	<title>Antrag #{app.id} - Risikomanagement</title>
</svelte:head>

<RoleGuard requiredRole="applicant">
	<div class="space-y-6">
		<div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
			<div class="flex items-center gap-4">
				<a href="/applications" class="back-link inline-flex items-center">
					<ArrowLeft class="w-5 h-5 mr-1" />
					Zurück
				</a>
				<h1 class="text-2xl font-bold text-primary" data-testid="application-title">
					Antrag #{app.id}
				</h1>
				<div data-testid="status-badge-container">
					<StatusBadge status={app.status} />
				</div>
			</div>
			{#if app.status === 'draft'}
				<div class="flex flex-col sm:flex-row gap-3 w-full sm:w-auto">
					<a
						href="/applications/{app.id}/edit"
						class="btn-secondary inline-flex items-center px-4 py-2 w-full sm:w-auto"
						data-testid="edit-application"
					>
						<Edit class="w-4 h-4 mr-2" />
						Bearbeiten
					</a>
					<button
						data-testid="submit-application"
						onclick={handleOpenConfirm}
						class="btn-primary inline-flex items-center px-4 py-2 w-full sm:w-auto"
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

		{#if showInquiryAnsweredMessage}
			<div
				class="success-message rounded-lg p-4"
				data-testid="application-inquiry-answered-message"
			>
				<p class="font-medium">Ihre Antwort wurde gespeichert und der Antrag erneut eingereicht.</p>
			</div>
		{/if}

		{#if isProcessing}
			<div class="card p-8 flex flex-col items-center gap-4 text-center" data-testid="processing-indicator">
				<Loader2 class="w-10 h-10 text-info animate-spin" />
				<p class="text-lg font-medium text-primary">Antrag wird verarbeitet...</p>
				<p class="text-sm text-secondary">Kundendaten werden abgerufen und Bonität geprüft. Dies kann einen Moment dauern.</p>
			</div>
		{/if}

		{#if isFailed}
			<div class="card p-6 border-danger" data-testid="failure-indicator">
				<div class="flex items-start gap-4">
					<AlertTriangle class="w-6 h-6 text-danger shrink-0 mt-0.5" />
					<div class="flex-1">
						<p class="text-lg font-medium text-danger">Verarbeitung fehlgeschlagen</p>
						{#if app.failureReason}
							<p class="mt-2 text-sm text-secondary" data-testid="failure-reason">{app.failureReason}</p>
						{/if}
						<div class="mt-4 flex flex-col sm:flex-row gap-3">
							<button
								onclick={handleRetry}
								disabled={isRetrying}
								class="btn-primary inline-flex items-center px-4 py-2"
								data-testid="retry-application"
							>
								<span class={isRetrying ? 'animate-spin' : ''}><RefreshCw class="w-4 h-4 mr-2" /></span>
								{isRetrying ? 'Wird erstellt...' : 'Erneut versuchen'}
							</button>
							<button
								onclick={handleDeleteFailed}
								class="btn-secondary inline-flex items-center px-4 py-2 text-danger border-danger hover:bg-danger/10"
								data-testid="delete-failed-application"
							>
								<Trash2 class="w-4 h-4 mr-2" />
								Löschen
							</button>
						</div>
					</div>
				</div>
			</div>
		{/if}

		<div class="grid grid-cols-1 md:grid-cols-1 lg:grid-cols-3 gap-6">
			<div class="lg:col-span-2 space-y-6">
				<div class="card p-6">
					<h2 class="text-lg font-semibold text-primary mb-4">Persönliche Daten</h2>
					<dl class="grid grid-cols-1 sm:grid-cols-2 gap-4">
						<div>
							<dt class="dl-label">Kunde</dt>
							<dd class="mt-1 dl-value">
								{#if app.customerName}
									<a href="/customers/{app.customerId}" class="text-brand-primary hover:text-brand-primary-hover">
										{app.customerName}
									</a>
								{:else}
									-
								{/if}
							</dd>
						</div>
						<div>
							<dt class="dl-label">Beschäftigungsstatus</dt>
							<dd class="mt-1 dl-value">{employmentStatusLabels[app.employmentStatus]}</dd>
						</div>
						<div>
							<dt class="dl-label">Zahlungsverzug in der Vergangenheit</dt>
							<dd class="mt-1 dl-value">{app.hasPaymentDefault == null ? '-' : app.hasPaymentDefault ? 'Ja' : 'Nein'}</dd>
						</div>
						<div>
							<dt class="dl-label">Credit Score</dt>
							<dd class="mt-1 dl-value" data-testid="application-credit-score">{app.creditScore ?? '-'}</dd>
						</div>
					</dl>
				</div>

				<div class="card p-6">
					<h2 class="text-lg font-semibold text-primary mb-4">Finanzielle Details</h2>
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
							<dd class="mt-1 dl-value font-medium">
								{formatCurrency(app.income - app.fixedCosts)}
							</dd>
						</div>
						<div class="sm:col-span-2">
							<dt class="dl-label">Gewünschte Rate</dt>
							<dd class="mt-1 text-lg text-primary font-semibold">
								{formatCurrency(app.desiredRate)}
							</dd>
						</div>
					</dl>
				</div>

				<div class="card p-6">
					<h2 class="text-lg font-semibold text-primary mb-4">Zeitstempel</h2>
					<dl class="grid grid-cols-1 sm:grid-cols-2 gap-4">
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

				<div class="card p-6" data-testid="application-inquiry-history">
					<h2 class="text-lg font-semibold text-primary mb-4">Rückfragenverlauf</h2>
					{#if inquiries.length === 0}
						<p class="text-secondary">Für diesen Antrag liegen noch keine Rückfragen vor.</p>
					{:else}
						<div class="space-y-4">
							{#each inquiries as inquiry}
								<div
									class="rounded-lg border border-default p-4"
									data-testid="application-inquiry-item"
								>
									<div class="flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between">
										<p class="font-medium text-primary">Rückfrage des Bearbeiters</p>
										<span class="text-xs text-secondary">
											{formatDate(inquiry.createdAt ? inquiry.createdAt.toString() : null)}
										</span>
									</div>
									<p class="mt-2 text-secondary">{inquiry.inquiryText}</p>
									{#if inquiry.responseText}
										<div
											class="mt-4 rounded-md bg-bg-muted p-3"
											data-testid="application-inquiry-response"
										>
											<p class="font-medium text-primary">Ihre Antwort</p>
											<p class="mt-1 text-secondary">{inquiry.responseText}</p>
										</div>
									{/if}
								</div>
							{/each}
						</div>
					{/if}
				</div>

				{#if canAnswerInquiry}
					<div class="card p-6" data-testid="application-inquiry-response-card">
						<h2 class="text-lg font-semibold text-primary mb-4">Auf Rückfrage antworten</h2>
						<form onsubmit={handleAnswerInquiry} class="space-y-4">
							<div>
								<label for="responseText" class="form-label block">Antwort</label>
								<textarea
									id="responseText"
									name="responseText"
									rows="4"
									bind:value={responseText}
									class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
									data-testid="application-inquiry-response-input"
									placeholder="Bitte beantworten Sie die Rückfrage des Bearbeiters"
								></textarea>
								{#if answerInquiryErrors?.responseText}
									<p class="mt-1 error-text" data-testid="application-inquiry-response-error">
										{answerInquiryErrors.responseText[0]}
									</p>
								{/if}
							</div>
							<div class="flex justify-end">
								<button
									type="submit"
									class="btn-primary px-4 py-2"
									data-testid="application-inquiry-response-submit"
								>
									Antwort absenden
								</button>
							</div>
						</form>
					</div>
				{/if}
			</div>

			<div class="space-y-6">
				<div class="card p-6">
					<h2 class="text-lg font-semibold text-primary mb-4" data-testid="scoring-heading">
						Bewertung
					</h2>
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
