<script lang="ts">
	import { goto } from '$app/navigation';
	import ConfirmDialog from '$lib/components/ConfirmDialog.svelte';
	import type { Application, Customer, EmploymentStatus } from '$lib/types';
	import { employmentStatusLabels } from '$lib/types';

	interface Props {
		application?: Application | null;
		errors?: Record<string, string[]>;
		isSubmitting?: boolean;
	}

	let { application = null, errors = $bindable({}), isSubmitting = $bindable(false) }: Props = $props();

	const employmentOptions: { value: EmploymentStatus; label: string }[] = [
		{ value: 'employed', label: employmentStatusLabels.employed },
		{ value: 'self_employed', label: employmentStatusLabels.self_employed },
		{ value: 'unemployed', label: employmentStatusLabels.unemployed },
		{ value: 'retired', label: employmentStatusLabels.retired }
	];

	let showConfirmDialog = $state(false);
	let pendingAction = $state<'submit' | null>(null);
	let generalError = $state('');
	let formRef: HTMLFormElement | null = null;
	let customers = $state<Customer[]>([]);
	let customersLoading = $state(true);

	$effect(() => {
		fetchActiveCustomers();
	});

	async function fetchActiveCustomers() {
		try {
			const res = await fetch('/api/customers/active');
			if (res.ok) {
				const body = await res.json();
				customers = body.customers ?? body;
			}
		} finally {
			customersLoading = false;
		}
	}

	function getFormData(): Record<string, unknown> {
		if (!formRef) return {};
		const fd = new FormData(formRef);
		return {
			customerId: parseInt(fd.get('customerId') as string, 10),
			income: parseFloat(fd.get('income') as string),
			fixedCosts: parseFloat(fd.get('fixedCosts') as string),
			desiredRate: parseFloat(fd.get('desiredRate') as string),
			employmentStatus: fd.get('employmentStatus') as string,
			hasPaymentDefault: fd.get('hasPaymentDefault') === 'true'
		};
	}

	async function submitForm(action?: string) {
		if (isSubmitting) return;
		if (formRef && !formRef.reportValidity()) return;

		isSubmitting = true;
		errors = {};
		generalError = '';

		const data = getFormData();

		try {
			const isEdit = !!application?.id;
			const baseUrl = isEdit ? `/api/applications/${application!.id}` : '/api/applications';
			const url = action === 'submit' ? `${baseUrl}?submit=true` : baseUrl;
			const method = isEdit ? 'PUT' : 'POST';

			const res = await fetch(url, {
				method,
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(data)
			});

			if (!res.ok) {
				const result = await res.json();
				if (result.errors) {
					errors = result.errors;
				} else if (result.error) {
					generalError = result.error;
				} else {
					generalError = 'Ein unerwarteter Fehler ist aufgetreten.';
				}
				return;
			}

			const result = await res.json();
			if (result.application?.id) {
				await goto(`/applications/${result.application.id}`);
			}
		} finally {
			isSubmitting = false;
		}
	}

	function handleSaveClick() {
		submitForm('save');
	}

	function handleSubmitClick(event: Event) {
		event.preventDefault();
		if (isSubmitting) return;
		if (formRef && !formRef.reportValidity()) return;
		pendingAction = 'submit';
		showConfirmDialog = true;
	}

	function handleConfirmSubmit() {
		if (pendingAction === 'submit') {
			showConfirmDialog = false;
			pendingAction = null;
			submitForm('submit');
		}
	}

	function handleCancelSubmit() {
		showConfirmDialog = false;
		pendingAction = null;
	}
</script>

<form onsubmit={(e) => { e.preventDefault(); handleSaveClick(); }} class="space-y-6" data-testid="application-form" bind:this={formRef}>
	{#if application?.id}
		<input type="hidden" name="id" value={application.id} />
	{/if}

	{#if generalError}
		<div class="rounded-md bg-danger/10 p-4" data-testid="application-general-error">
			<p class="text-sm text-danger">{generalError}</p>
		</div>
	{/if}

	{#if Object.keys(errors).length > 0}
		<div class="rounded-md bg-danger/10 p-4" data-testid="application-validation-summary">
			<p class="text-sm font-medium text-danger">Bitte korrigieren Sie die folgenden Fehler:</p>
			<ul class="mt-2 list-disc list-inside text-sm text-danger">
				{#each Object.values(errors).flat() as msg}
					<li>{msg}</li>
				{/each}
			</ul>
		</div>
	{/if}

	<div>
		<label for="customerId" class="form-label block">Kunde</label>
		{#if customersLoading}
			<div class="mt-1 text-sm text-secondary">Kunden werden geladen...</div>
		{:else}
			<select
				id="customerId"
				name="customerId"
				required
				class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
				data-testid="select-customer"
			>
				<option value="">Bitte Kunde wählen...</option>
				{#each customers as c}
					<option value={c.id} selected={application?.customerId === c.id}>
						{c.lastName}, {c.firstName}
					</option>
				{/each}
			</select>
		{/if}
		{#if customers.length === 0 && !customersLoading}
			<p class="mt-1 text-sm text-secondary">
				Keine aktiven Kunden vorhanden.
				<a href="/customers/new" class="text-brand-primary hover:text-brand-primary-hover">Neuen Kunden anlegen</a>
			</p>
		{/if}
		{#if errors.customerId}
			<p class="mt-1 error-text" data-testid="customer-error">{errors.customerId[0]}</p>
		{/if}
	</div>

	<div class="grid grid-cols-1 gap-6 sm:grid-cols-2 md:grid-cols-3">
		<div>
			<label for="income" class="form-label block">Monatliches Einkommen (EUR)</label>
			<input
				type="number"
				id="income"
				name="income"
				value={application?.income ?? ''}
				required
				min="0"
				step="0.01"
				class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
				placeholder="z.B. 3500"
				data-testid="input-income"
			/>
			{#if errors.income}
				<p class="mt-1 error-text">{errors.income[0]}</p>
			{/if}
		</div>

		<div>
			<label for="fixedCosts" class="form-label block">Monatliche Fixkosten (EUR)</label>
			<input
				type="number"
				id="fixedCosts"
				name="fixedCosts"
				value={application?.fixedCosts ?? ''}
				required
				min="0"
				step="0.01"
				class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
				placeholder="z.B. 1500"
				data-testid="input-fixed-costs"
			/>
			{#if errors.fixedCosts}
				<p class="mt-1 error-text">{errors.fixedCosts[0]}</p>
			{/if}
		</div>

		<div>
			<label for="desiredRate" class="form-label block">Gewünschte Rate (EUR)</label>
			<input
				type="number"
				id="desiredRate"
				name="desiredRate"
				value={application?.desiredRate ?? ''}
				required
				min="0"
				step="0.01"
				class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
				placeholder="z.B. 500"
				data-testid="input-desired-rate"
			/>
			{#if errors.desiredRate}
				<p class="mt-1 error-text">{errors.desiredRate[0]}</p>
			{/if}
		</div>
	</div>

	<div>
		<label for="employmentStatus" class="form-label block">Beschäftigungsstatus</label>
		<select
			id="employmentStatus"
			name="employmentStatus"
			required
			class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
			data-testid="select-employment-status"
		>
			<option value="">Bitte wählen...</option>
			{#each employmentOptions as option}
				<option value={option.value} selected={application?.employmentStatus === option.value}>
					{option.label}
				</option>
			{/each}
		</select>
		{#if errors.employmentStatus}
			<p class="mt-1 error-text">{errors.employmentStatus[0]}</p>
		{/if}
	</div>

	<div>
		<fieldset>
			<legend class="form-label block">Zahlungsverzug in der Vergangenheit?</legend>
			<div class="mt-2 flex flex-col sm:flex-row gap-4 sm:gap-6">
				<label class="inline-flex items-center">
					<input
						type="radio"
						name="hasPaymentDefault"
						value="true"
						checked={application?.hasPaymentDefault === true}
						class="h-4 w-4 border-default"
						data-testid="radio-payment-default-yes"
					/>
					<span class="ml-2 text-sm text-primary">Ja</span>
				</label>
				<label class="inline-flex items-center">
					<input
						type="radio"
						name="hasPaymentDefault"
						value="false"
						checked={application?.hasPaymentDefault === false}
						class="h-4 w-4 border-default"
						data-testid="radio-payment-default-no"
					/>
					<span class="ml-2 text-sm text-primary">Nein</span>
				</label>
			</div>
		</fieldset>
		{#if errors.hasPaymentDefault}
			<p class="mt-1 error-text">{errors.hasPaymentDefault[0]}</p>
		{/if}
	</div>

	<div class="flex flex-col sm:flex-row justify-end gap-3 pt-4 border-t border-default">
		<button
			type="submit"
			name="action"
			value="save"
			disabled={isSubmitting}
			class="btn-secondary w-full sm:w-auto"
			data-testid="btn-save-draft"
		>
			Als Entwurf speichern
		</button>
		<button
			type="button"
			name="action"
			value="submit"
			disabled={isSubmitting}
			class="btn-primary w-full sm:w-auto"
			data-testid="btn-submit-application"
			onclick={handleSubmitClick}
		>
			Antrag einreichen
		</button>
	</div>

	<ConfirmDialog
		open={showConfirmDialog}
		message="Möchten Sie diesen Antrag wirklich einreichen? Nach der Einreichung ist keine Bearbeitung mehr möglich."
		confirmText="Antrag einreichen"
		cancelText="Abbrechen"
		onConfirm={handleConfirmSubmit}
		onCancel={handleCancelSubmit}
	/>
</form>
