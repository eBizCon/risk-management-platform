<script lang="ts">
	import { goto } from '$app/navigation';
	import ConfirmDialog from '$lib/components/ConfirmDialog.svelte';
	import RoleGuard from '$lib/components/RoleGuard.svelte';
	import { customerStatusLabels, employmentStatusLabels } from '$lib/types';
	import { ArrowLeft, Edit, Archive, RotateCcw, Trash2 } from 'lucide-svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	const customer = $derived(data.customer);

	let showDeleteDialog = $state(false);
	let showArchiveDialog = $state(false);
	let creditCheckLoading = $state(false);

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

	function handleDeleteRequest() {
		showDeleteDialog = true;
	}

	async function handleConfirmDelete() {
		showDeleteDialog = false;
		const response = await fetch(`/api/customers/${customer.id}`, { method: 'DELETE' });
		if (response.ok) {
			await goto('/customers');
		}
	}

	function handleCancelDelete() {
		showDeleteDialog = false;
	}

	function handleArchiveRequest() {
		showArchiveDialog = true;
	}

	async function handleConfirmArchive() {
		showArchiveDialog = false;
		const response = await fetch(`/api/customers/${customer.id}/archive`, { method: 'POST' });
		if (response.ok) {
			window.location.reload();
		}
	}

	function handleCancelArchive() {
		showArchiveDialog = false;
	}

	async function handleActivate() {
		const response = await fetch(`/api/customers/${customer.id}/activate`, { method: 'POST' });
		if (response.ok) {
			window.location.reload();
		}
	}

	async function handleRequestCreditReport() {
		if (creditCheckLoading) return;
		creditCheckLoading = true;
		try {
			const response = await fetch(`/api/customers/${customer.id}/credit-report`, { method: 'POST' });
			if (response.ok) {
				window.location.reload();
			}
		} finally {
			creditCheckLoading = false;
		}
	}
</script>

<svelte:head>
	<title>{customer.lastName}, {customer.firstName} - Risikomanagement</title>
</svelte:head>

<RoleGuard requiredRole="applicant">
	<div class="space-y-6">
		<div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
			<div class="flex items-center gap-4">
				<a href="/customers" class="back-link inline-flex items-center">
					<ArrowLeft class="w-5 h-5 mr-1" />
					Zurück
				</a>
				<h1 class="text-2xl font-bold text-primary" data-testid="customer-detail-title">
					{customer.lastName}, {customer.firstName}
				</h1>
				<span
					class="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium"
					class:bg-success={customer.status === 'active'}
					class:text-white={customer.status === 'active'}
					class:bg-bg-muted={customer.status === 'archived'}
					class:text-secondary={customer.status === 'archived'}
					data-testid="customer-detail-status"
				>
					{customerStatusLabels[customer.status] ?? customer.status}
				</span>
			</div>
			<div class="flex flex-col sm:flex-row gap-3 w-full sm:w-auto">
				{#if customer.status === 'active'}
					<a
						href="/customers/{customer.id}/edit"
						class="btn-secondary inline-flex items-center px-4 py-2 w-full sm:w-auto"
						data-testid="customer-edit-btn"
					>
						<Edit class="w-4 h-4 mr-2" />
						Bearbeiten
					</a>
					<button
						onclick={handleArchiveRequest}
						class="btn-secondary inline-flex items-center px-4 py-2 w-full sm:w-auto text-warning"
						data-testid="customer-archive-btn"
					>
						<Archive class="w-4 h-4 mr-2" />
						Archivieren
					</button>
				{:else}
					<button
						onclick={handleActivate}
						class="btn-secondary inline-flex items-center px-4 py-2 w-full sm:w-auto text-success"
						data-testid="customer-activate-btn"
					>
						<RotateCcw class="w-4 h-4 mr-2" />
						Aktivieren
					</button>
				{/if}
				<button
					onclick={handleDeleteRequest}
					class="btn-secondary inline-flex items-center px-4 py-2 w-full sm:w-auto text-danger"
					data-testid="customer-delete-btn"
				>
					<Trash2 class="w-4 h-4 mr-2" />
					Löschen
				</button>
			</div>
		</div>

		<div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
			<div class="card p-6">
				<h2 class="text-lg font-semibold text-primary mb-4">Persönliche Daten</h2>
				<dl class="grid grid-cols-1 sm:grid-cols-2 gap-4">
					<div>
						<dt class="dl-label">Vorname</dt>
						<dd class="mt-1 dl-value" data-testid="customer-detail-firstName">{customer.firstName}</dd>
					</div>
					<div>
						<dt class="dl-label">Nachname</dt>
						<dd class="mt-1 dl-value" data-testid="customer-detail-lastName">{customer.lastName}</dd>
					</div>
					<div>
						<dt class="dl-label">E-Mail</dt>
						<dd class="mt-1 dl-value" data-testid="customer-detail-email">{customer.email ?? '-'}</dd>
					</div>
					<div>
						<dt class="dl-label">Telefon</dt>
						<dd class="mt-1 dl-value" data-testid="customer-detail-phone">{customer.phone}</dd>
					</div>
					<div>
						<dt class="dl-label">Geburtsdatum</dt>
						<dd class="mt-1 dl-value" data-testid="customer-detail-dateOfBirth">{customer.dateOfBirth}</dd>
					</div>
					<div>
						<dt class="dl-label">Beschäftigungsstatus</dt>
						<dd class="mt-1 dl-value" data-testid="customer-detail-employmentStatus">{employmentStatusLabels[customer.employmentStatus] ?? customer.employmentStatus}</dd>
					</div>
				</dl>
			</div>

			<div class="card p-6">
				<h2 class="text-lg font-semibold text-primary mb-4">Adresse</h2>
				<dl class="grid grid-cols-1 sm:grid-cols-2 gap-4">
					<div class="sm:col-span-2">
						<dt class="dl-label">Straße</dt>
						<dd class="mt-1 dl-value" data-testid="customer-detail-street">{customer.street}</dd>
					</div>
					<div>
						<dt class="dl-label">PLZ</dt>
						<dd class="mt-1 dl-value" data-testid="customer-detail-zipCode">{customer.zipCode}</dd>
					</div>
					<div>
						<dt class="dl-label">Stadt</dt>
						<dd class="mt-1 dl-value" data-testid="customer-detail-city">{customer.city}</dd>
					</div>
					<div>
						<dt class="dl-label">Land</dt>
						<dd class="mt-1 dl-value" data-testid="customer-detail-country">{customer.country}</dd>
					</div>
				</dl>
			</div>

			<div class="card p-6 lg:col-span-2" data-testid="customer-credit-report-section">
				<div class="flex flex-col sm:flex-row sm:items-center sm:justify-between mb-4">
					<h2 class="text-lg font-semibold text-primary">Bonität</h2>
					{#if customer.status === 'active'}
						<button
							onclick={handleRequestCreditReport}
							disabled={creditCheckLoading}
							class="btn-secondary px-4 py-2 mt-2 sm:mt-0"
							data-testid="customer-credit-check-btn"
						>
							{creditCheckLoading ? 'Prüfung läuft...' : 'Bonität prüfen'}
						</button>
					{/if}
				</div>
				{#if customer.creditReport}
					<dl class="grid grid-cols-1 sm:grid-cols-2 gap-4">
						<div>
							<dt class="dl-label">Zahlungsverzug</dt>
							<dd class="mt-1 dl-value" data-testid="customer-detail-hasPaymentDefault">{customer.creditReport.hasPaymentDefault ? 'Ja' : 'Nein'}</dd>
						</div>
						<div>
							<dt class="dl-label">Credit Score</dt>
							<dd class="mt-1 dl-value" data-testid="customer-detail-creditScore">{customer.creditReport.creditScore ?? '-'}</dd>
						</div>
						<div>
							<dt class="dl-label">Geprüft am</dt>
							<dd class="mt-1 dl-value" data-testid="customer-detail-creditCheckedAt">{formatDate(customer.creditReport.checkedAt)}</dd>
						</div>
						<div>
							<dt class="dl-label">Anbieter</dt>
							<dd class="mt-1 dl-value" data-testid="customer-detail-creditProvider">{customer.creditReport.provider}</dd>
						</div>
					</dl>
				{:else}
					<p class="text-secondary" data-testid="customer-no-credit-report">Keine Bonitätsprüfung vorhanden</p>
				{/if}
			</div>

			<div class="card p-6 lg:col-span-2">
				<h2 class="text-lg font-semibold text-primary mb-4">Zeitstempel</h2>
				<dl class="grid grid-cols-1 sm:grid-cols-3 gap-4">
					<div>
						<dt class="dl-label">Erstellt am</dt>
						<dd class="mt-1 dl-value">{formatDate(customer.createdAt)}</dd>
					</div>
					<div>
						<dt class="dl-label">Zuletzt geändert</dt>
						<dd class="mt-1 dl-value">{formatDate(customer.updatedAt)}</dd>
					</div>
					<div>
						<dt class="dl-label">Erstellt von</dt>
						<dd class="mt-1 dl-value">{customer.createdBy}</dd>
					</div>
				</dl>
			</div>
		</div>
	</div>

	<ConfirmDialog
		open={showDeleteDialog}
		message="Möchten Sie diesen Kunden wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden."
		confirmText="Löschen"
		cancelText="Abbrechen"
		onConfirm={handleConfirmDelete}
		onCancel={handleCancelDelete}
	/>

	<ConfirmDialog
		open={showArchiveDialog}
		message="Möchten Sie diesen Kunden wirklich archivieren?"
		confirmText="Archivieren"
		cancelText="Abbrechen"
		onConfirm={handleConfirmArchive}
		onCancel={handleCancelArchive}
	/>
</RoleGuard>
