<script lang="ts">
	import { goto } from '$app/navigation';
	import type { Customer } from '$lib/types';
	import { employmentStatusLabels } from '$lib/types';

	interface Props {
		customer?: Customer;
	}

	let { customer }: Props = $props();

	let isSubmitting = $state(false);
	let errors = $state<Record<string, string[]>>({});
	let generalError = $state('');
	let formRef = $state<HTMLFormElement | null>(null);

	function getFormData(): Record<string, unknown> {
		if (!formRef) return {};
		const fd = new FormData(formRef);
		return {
			firstName: fd.get('firstName') as string,
			lastName: fd.get('lastName') as string,
			email: (fd.get('email') as string) || null,
			phone: fd.get('phone') as string,
			dateOfBirth: fd.get('dateOfBirth') as string,
			street: fd.get('street') as string,
			city: fd.get('city') as string,
			zipCode: fd.get('zipCode') as string,
			country: fd.get('country') as string,
			employmentStatus: fd.get('employmentStatus') as string
		};
	}

	async function submitForm() {
		if (isSubmitting) return;
		if (formRef && !formRef.reportValidity()) return;

		isSubmitting = true;
		errors = {};
		generalError = '';

		const data = getFormData();

		try {
			const isEdit = !!customer?.id;
			const url = isEdit ? `/api/customers/${customer!.id}` : '/api/customers';
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
			const customerId = result.customer?.id ?? result.id;
			if (customerId) {
				await goto(`/customers/${customerId}`);
			} else {
				await goto('/customers');
			}
		} finally {
			isSubmitting = false;
		}
	}
</script>

<form
	bind:this={formRef}
	onsubmit={(e) => {
		e.preventDefault();
		submitForm();
	}}
	class="space-y-6"
	data-testid="customer-form"
>
	{#if generalError}
		<div class="rounded-md bg-danger/10 p-4" data-testid="customer-general-error">
			<p class="text-sm text-danger">{generalError}</p>
		</div>
	{/if}

	{#if Object.keys(errors).length > 0}
		<div class="rounded-md bg-danger/10 p-4" data-testid="customer-validation-summary">
			<p class="text-sm font-medium text-danger">Bitte korrigieren Sie die folgenden Fehler:</p>
			<ul class="mt-2 list-disc list-inside text-sm text-danger">
				{#each Object.values(errors).flat() as msg}
					<li>{msg}</li>
				{/each}
			</ul>
		</div>
	{/if}

	<div class="card p-6">
		<h2 class="text-lg font-semibold text-primary mb-4">Persönliche Daten</h2>
		<div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
			<div>
				<label for="firstName" class="form-label block">Vorname</label>
				<input
					type="text"
					id="firstName"
					name="firstName"
					value={customer?.firstName ?? ''}
					required
					class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
					data-testid="customer-firstName"
				/>
				{#if errors.firstName}
					<p class="mt-1 error-text" data-testid="customer-firstName-error">
						{errors.firstName[0]}
					</p>
				{/if}
			</div>
			<div>
				<label for="lastName" class="form-label block">Nachname</label>
				<input
					type="text"
					id="lastName"
					name="lastName"
					value={customer?.lastName ?? ''}
					required
					class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
					data-testid="customer-lastName"
				/>
				{#if errors.lastName}
					<p class="mt-1 error-text" data-testid="customer-lastName-error">{errors.lastName[0]}</p>
				{/if}
			</div>
			<div>
				<label for="email" class="form-label block">E-Mail</label>
				<input
					type="email"
					id="email"
					name="email"
					value={customer?.email ?? ''}
					class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
					data-testid="customer-email"
				/>
				{#if errors.email}
					<p class="mt-1 error-text" data-testid="customer-email-error">{errors.email[0]}</p>
				{/if}
			</div>
			<div>
				<label for="phone" class="form-label block">Telefon</label>
				<input
					type="tel"
					id="phone"
					name="phone"
					value={customer?.phone ?? ''}
					required
					class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
					data-testid="customer-phone"
				/>
				{#if errors.phone}
					<p class="mt-1 error-text" data-testid="customer-phone-error">{errors.phone[0]}</p>
				{/if}
			</div>
			<div>
				<label for="dateOfBirth" class="form-label block">Geburtsdatum</label>
				<input
					type="date"
					id="dateOfBirth"
					name="dateOfBirth"
					value={customer?.dateOfBirth ?? ''}
					required
					class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
					data-testid="customer-dateOfBirth"
				/>
				{#if errors.dateOfBirth}
					<p class="mt-1 error-text" data-testid="customer-dateOfBirth-error">
						{errors.dateOfBirth[0]}
					</p>
				{/if}
			</div>
		</div>
	</div>

	<div class="card p-6">
		<h2 class="text-lg font-semibold text-primary mb-4">Beschäftigung</h2>
		<div>
			<label for="employmentStatus" class="form-label block">Beschäftigungsstatus</label>
			<select
				id="employmentStatus"
				name="employmentStatus"
				required
				class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
				data-testid="customer-employmentStatus"
			>
				<option value="">Bitte wählen...</option>
				<option value="employed" selected={customer?.employmentStatus === 'employed'}
					>{employmentStatusLabels.employed}</option
				>
				<option value="self_employed" selected={customer?.employmentStatus === 'self_employed'}
					>{employmentStatusLabels.self_employed}</option
				>
				<option value="unemployed" selected={customer?.employmentStatus === 'unemployed'}
					>{employmentStatusLabels.unemployed}</option
				>
				<option value="retired" selected={customer?.employmentStatus === 'retired'}
					>{employmentStatusLabels.retired}</option
				>
			</select>
			{#if errors.employmentStatus}
				<p class="mt-1 error-text" data-testid="customer-employmentStatus-error">
					{errors.employmentStatus[0]}
				</p>
			{/if}
		</div>
	</div>

	<div class="card p-6">
		<h2 class="text-lg font-semibold text-primary mb-4">Adresse</h2>
		<div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
			<div class="sm:col-span-2">
				<label for="street" class="form-label block">Straße</label>
				<input
					type="text"
					id="street"
					name="street"
					value={customer?.street ?? ''}
					required
					class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
					data-testid="customer-street"
				/>
				{#if errors.street}
					<p class="mt-1 error-text" data-testid="customer-street-error">{errors.street[0]}</p>
				{/if}
			</div>
			<div>
				<label for="zipCode" class="form-label block">PLZ</label>
				<input
					type="text"
					id="zipCode"
					name="zipCode"
					value={customer?.zipCode ?? ''}
					required
					class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
					data-testid="customer-zipCode"
				/>
				{#if errors.zipCode}
					<p class="mt-1 error-text" data-testid="customer-zipCode-error">{errors.zipCode[0]}</p>
				{/if}
			</div>
			<div>
				<label for="city" class="form-label block">Stadt</label>
				<input
					type="text"
					id="city"
					name="city"
					value={customer?.city ?? ''}
					required
					class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
					data-testid="customer-city"
				/>
				{#if errors.city}
					<p class="mt-1 error-text" data-testid="customer-city-error">{errors.city[0]}</p>
				{/if}
			</div>
			<div>
				<label for="country" class="form-label block">Land</label>
				<input
					type="text"
					id="country"
					name="country"
					value={customer?.country ?? 'Deutschland'}
					required
					class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
					data-testid="customer-country"
				/>
				{#if errors.country}
					<p class="mt-1 error-text" data-testid="customer-country-error">{errors.country[0]}</p>
				{/if}
			</div>
		</div>
	</div>

	<div class="flex flex-col sm:flex-row justify-end gap-3">
		<a
			href={customer?.id ? `/customers/${customer.id}` : '/customers'}
			class="btn-secondary px-4 py-2 text-center"
			data-testid="customer-cancel"
		>
			Abbrechen
		</a>
		<button
			type="submit"
			disabled={isSubmitting}
			class="btn-primary px-4 py-2"
			data-testid="customer-submit"
		>
			{isSubmitting ? 'Speichern...' : customer?.id ? 'Änderungen speichern' : 'Kunde anlegen'}
		</button>
	</div>
</form>
