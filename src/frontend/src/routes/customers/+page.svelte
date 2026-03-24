<script lang="ts">
	import { goto } from '$app/navigation';
	import CustomerTable from '$lib/components/CustomerTable.svelte';
	import ConfirmDialog from '$lib/components/ConfirmDialog.svelte';
	import RoleGuard from '$lib/components/RoleGuard.svelte';
	import { Plus } from 'lucide-svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	let showDeleteDialog = $state(false);
	let showArchiveDialog = $state(false);
	let pendingActionId = $state<number | null>(null);

	function handleView(id: number) {
		goto(`/customers/${id}`);
	}

	function handleEdit(id: number) {
		goto(`/customers/${id}/edit`);
	}

	function handleDeleteRequest(id: number) {
		pendingActionId = id;
		showDeleteDialog = true;
	}

	async function handleConfirmDelete() {
		if (!pendingActionId) return;
		showDeleteDialog = false;
		const response = await fetch(`/api/customers/${pendingActionId}`, { method: 'DELETE' });
		if (response.ok) {
			window.location.reload();
		}
		pendingActionId = null;
	}

	function handleCancelDelete() {
		showDeleteDialog = false;
		pendingActionId = null;
	}

	function handleArchiveRequest(id: number) {
		pendingActionId = id;
		showArchiveDialog = true;
	}

	async function handleConfirmArchive() {
		if (!pendingActionId) return;
		showArchiveDialog = false;
		const response = await fetch(`/api/customers/${pendingActionId}/archive`, { method: 'POST' });
		if (response.ok) {
			window.location.reload();
		}
		pendingActionId = null;
	}

	function handleCancelArchive() {
		showArchiveDialog = false;
		pendingActionId = null;
	}

	async function handleActivate(id: number) {
		const response = await fetch(`/api/customers/${id}/activate`, { method: 'POST' });
		if (response.ok) {
			window.location.reload();
		}
	}
</script>

<svelte:head>
	<title>Meine Kunden - Risikomanagement</title>
</svelte:head>

<RoleGuard requiredRole="applicant">
	<div class="space-y-6">
		<div class="flex flex-col sm:flex-row sm:justify-between sm:items-center gap-4">
			<div>
				<h1 class="text-2xl font-bold text-primary" data-testid="customers-title">Meine Kunden</h1>
				<p class="text-secondary mt-1">Übersicht über alle Ihre Kunden</p>
			</div>
			<a
				href="/customers/new"
				class="btn-primary inline-flex items-center px-4 py-2 w-full sm:w-auto"
				data-testid="customer-new-btn"
			>
				<Plus class="w-5 h-5 mr-2" />
				Neuer Kunde
			</a>
		</div>

		<div class="card">
			<div class="px-4 py-4 border-b border-default">
				<span class="text-sm text-secondary">
					{data.customers.length} Kunde(n) gefunden
				</span>
			</div>

			<CustomerTable
				customers={data.customers}
				onView={handleView}
				onEdit={handleEdit}
				onDelete={handleDeleteRequest}
				onArchive={handleArchiveRequest}
				onActivate={handleActivate}
			/>
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
