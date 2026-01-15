<script lang="ts">
	import type { Application, EmploymentStatus } from '$lib/types';
	import { employmentStatusLabels } from '$lib/types';
	import { enhance } from '$app/forms';

	interface Props {
		application?: Application | null;
		errors?: Record<string, string[]>;
		isSubmitting?: boolean;
	}

	let { application = null, errors = {}, isSubmitting = false }: Props = $props();

	const employmentOptions: { value: EmploymentStatus; label: string }[] = [
		{ value: 'employed', label: employmentStatusLabels.employed },
		{ value: 'self_employed', label: employmentStatusLabels.self_employed },
		{ value: 'unemployed', label: employmentStatusLabels.unemployed },
		{ value: 'retired', label: employmentStatusLabels.retired }
	];
</script>

<form method="POST" use:enhance class="space-y-6">
	{#if application?.id}
		<input type="hidden" name="id" value={application.id} />
	{/if}

	<div>
		<label for="name" class="form-label block">Name</label>
		<input
			type="text"
			id="name"
			name="name"
			value={application?.name ?? ''}
			required
			class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
			placeholder="Vor- und Nachname"
		/>
		{#if errors.name}
			<p class="mt-1 error-text">{errors.name[0]}</p>
		{/if}
	</div>

	<div class="grid grid-cols-1 gap-6 sm:grid-cols-3">
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
			<div class="mt-2 space-x-6 flex">
				<label class="inline-flex items-center">
					<input
						type="radio"
						name="hasPaymentDefault"
						value="true"
						checked={application?.hasPaymentDefault === true}
						class="h-4 w-4 border-default"
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
					/>
					<span class="ml-2 text-sm text-primary">Nein</span>
				</label>
			</div>
		</fieldset>
		{#if errors.hasPaymentDefault}
			<p class="mt-1 error-text">{errors.hasPaymentDefault[0]}</p>
		{/if}
	</div>

	<div class="flex justify-end gap-3 pt-4 border-t border-default">
		<button
			type="submit"
			name="action"
			value="save"
			disabled={isSubmitting}
			class="btn-secondary"
		>
			Als Entwurf speichern
		</button>
		<button
			type="submit"
			name="action"
			value="submit"
			disabled={isSubmitting}
			class="btn-primary"
		>
			Antrag einreichen
		</button>
	</div>
</form>
