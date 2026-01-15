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
		<label for="name" class="block text-sm font-medium text-gray-700">Name</label>
		<input
			type="text"
			id="name"
			name="name"
			value={application?.name ?? ''}
			required
			class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
			placeholder="Vor- und Nachname"
		/>
		{#if errors.name}
			<p class="mt-1 text-sm text-red-600">{errors.name[0]}</p>
		{/if}
	</div>

	<div class="grid grid-cols-1 gap-6 sm:grid-cols-3">
		<div>
			<label for="income" class="block text-sm font-medium text-gray-700">Monatliches Einkommen (EUR)</label>
			<input
				type="number"
				id="income"
				name="income"
				value={application?.income ?? ''}
				required
				min="0"
				step="0.01"
				class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
				placeholder="z.B. 3500"
			/>
			{#if errors.income}
				<p class="mt-1 text-sm text-red-600">{errors.income[0]}</p>
			{/if}
		</div>

		<div>
			<label for="fixedCosts" class="block text-sm font-medium text-gray-700">Monatliche Fixkosten (EUR)</label>
			<input
				type="number"
				id="fixedCosts"
				name="fixedCosts"
				value={application?.fixedCosts ?? ''}
				required
				min="0"
				step="0.01"
				class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
				placeholder="z.B. 1500"
			/>
			{#if errors.fixedCosts}
				<p class="mt-1 text-sm text-red-600">{errors.fixedCosts[0]}</p>
			{/if}
		</div>

		<div>
			<label for="desiredRate" class="block text-sm font-medium text-gray-700">Gewünschte Rate (EUR)</label>
			<input
				type="number"
				id="desiredRate"
				name="desiredRate"
				value={application?.desiredRate ?? ''}
				required
				min="0"
				step="0.01"
				class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
				placeholder="z.B. 500"
			/>
			{#if errors.desiredRate}
				<p class="mt-1 text-sm text-red-600">{errors.desiredRate[0]}</p>
			{/if}
		</div>
	</div>

	<div>
		<label for="employmentStatus" class="block text-sm font-medium text-gray-700">Beschäftigungsstatus</label>
		<select
			id="employmentStatus"
			name="employmentStatus"
			required
			class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
		>
			<option value="">Bitte wählen...</option>
			{#each employmentOptions as option}
				<option value={option.value} selected={application?.employmentStatus === option.value}>
					{option.label}
				</option>
			{/each}
		</select>
		{#if errors.employmentStatus}
			<p class="mt-1 text-sm text-red-600">{errors.employmentStatus[0]}</p>
		{/if}
	</div>

	<div>
		<fieldset>
			<legend class="block text-sm font-medium text-gray-700">Zahlungsverzug in der Vergangenheit?</legend>
			<div class="mt-2 space-x-6 flex">
				<label class="inline-flex items-center">
					<input
						type="radio"
						name="hasPaymentDefault"
						value="true"
						checked={application?.hasPaymentDefault === true}
						class="h-4 w-4 border-gray-300 text-indigo-600 focus:ring-indigo-500"
					/>
					<span class="ml-2 text-sm text-gray-700">Ja</span>
				</label>
				<label class="inline-flex items-center">
					<input
						type="radio"
						name="hasPaymentDefault"
						value="false"
						checked={application?.hasPaymentDefault === false}
						class="h-4 w-4 border-gray-300 text-indigo-600 focus:ring-indigo-500"
					/>
					<span class="ml-2 text-sm text-gray-700">Nein</span>
				</label>
			</div>
		</fieldset>
		{#if errors.hasPaymentDefault}
			<p class="mt-1 text-sm text-red-600">{errors.hasPaymentDefault[0]}</p>
		{/if}
	</div>

	<div class="flex justify-end gap-3 pt-4 border-t">
		<button
			type="submit"
			name="action"
			value="save"
			disabled={isSubmitting}
			class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50"
		>
			Als Entwurf speichern
		</button>
		<button
			type="submit"
			name="action"
			value="submit"
			disabled={isSubmitting}
			class="px-4 py-2 text-sm font-medium text-white bg-indigo-600 border border-transparent rounded-md shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50"
		>
			Antrag einreichen
		</button>
	</div>
</form>
