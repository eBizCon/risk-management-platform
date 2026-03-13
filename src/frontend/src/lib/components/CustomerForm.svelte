<script lang="ts">
	import { goto } from '$app/navigation';
	import type { Customer } from '$lib/types';

	interface Props {
		customer?: Customer;
	}

	let { customer }: Props = $props();

	let isSubmitting = $state(false);
	let errors = $state<Record<string, string[]>>({});
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
			country: fd.get('country') as string
		};
	}

	async function submitForm() {
		if (isSubmitting) return;
		if (formRef && !formRef.reportValidity()) return;

		isSubmitting = true;
		errors = {};

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
				}
				return;
			}

			const result = await res.json();
			if (result.id) {
				await goto(`/customers/${result.id}`);
			} else if (customer?.id) {
				await goto(`/customers/${customer.id}`);
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
	onsubmit={(e) => { e.preventDefault(); submitForm(); }}
	class="space-y-6"
	data-testid="customer-form"
>
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
				{#if errors.FirstName}
					<p class="mt-1 error-text" data-testid="customer-firstName-error">{errors.FirstName[0]}</p>
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
				{#if errors.LastName}
					<p class="mt-1 error-text" data-testid="customer-lastName-error">{errors.LastName[0]}</p>
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
				{#if errors.Email}
					<p class="mt-1 error-text" data-testid="customer-email-error">{errors.Email[0]}</p>
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
				{#if errors.Phone}
					<p class="mt-1 error-text" data-testid="customer-phone-error">{errors.Phone[0]}</p>
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
				{#if errors.DateOfBirth}
					<p class="mt-1 error-text" data-testid="customer-dateOfBirth-error">{errors.DateOfBirth[0]}</p>
				{/if}
			</div>
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
				{#if errors.Street}
					<p class="mt-1 error-text" data-testid="customer-street-error">{errors.Street[0]}</p>
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
				{#if errors.ZipCode}
					<p class="mt-1 error-text" data-testid="customer-zipCode-error">{errors.ZipCode[0]}</p>
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
				{#if errors.City}
					<p class="mt-1 error-text" data-testid="customer-city-error">{errors.City[0]}</p>
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
				{#if errors.Country}
					<p class="mt-1 error-text" data-testid="customer-country-error">{errors.Country[0]}</p>
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
