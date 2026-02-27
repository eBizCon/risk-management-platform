<script lang="ts">
	import RoleGuard from '$lib/components/RoleGuard.svelte';

	let { data, form } = $props();

	const config = $derived(form?.config ?? data.config);
	const success = $derived(form?.success === true);
	const errors = $derived(form?.errors as Record<string, string[]> | undefined);
</script>

<RoleGuard requiredRole="admin">
	<div class="space-y-6">
		<div>
			<h1 class="text-2xl font-bold text-primary">Scoring-Konfiguration</h1>
			<p class="mt-1 text-sm text-secondary">
				Verwalten Sie die Parameter für die automatische Risikobewertung.
			</p>
		</div>

		{#if success}
			<div
				class="rounded-md bg-green-50 p-4 border border-green-200"
				data-testid="success-message"
			>
				<p class="text-sm font-medium text-green-800">
					Konfiguration erfolgreich gespeichert.
				</p>
			</div>
		{/if}

		<form method="POST" class="space-y-6" data-testid="scoring-config-form">
			<div class="surface-bg shadow rounded-lg p-6 space-y-6">
				<h2 class="text-lg font-semibold text-primary border-b border-default pb-2">
					Ampel-Schwellenwerte
				</h2>

				<div class="grid grid-cols-1 md:grid-cols-2 gap-4">
					<div>
						<label for="greenThreshold" class="block text-sm font-medium text-primary">
							Grün-Schwellenwert (Score &ge;)
						</label>
						<input
							type="number"
							id="greenThreshold"
							name="greenThreshold"
							value={config.greenThreshold}
							min="1"
							max="100"
							class="mt-1 block w-full rounded-md border-default shadow-sm focus:border-brand focus:ring-brand sm:text-sm"
							data-testid="input-green-threshold"
						/>
						{#if errors?.greenThreshold}
							<p class="mt-1 text-sm text-red-600">{errors.greenThreshold[0]}</p>
						{/if}
					</div>

					<div>
						<label for="yellowThreshold" class="block text-sm font-medium text-primary">
							Gelb-Schwellenwert (Score &ge;)
						</label>
						<input
							type="number"
							id="yellowThreshold"
							name="yellowThreshold"
							value={config.yellowThreshold}
							min="1"
							max="100"
							class="mt-1 block w-full rounded-md border-default shadow-sm focus:border-brand focus:ring-brand sm:text-sm"
							data-testid="input-yellow-threshold"
						/>
						{#if errors?.yellowThreshold}
							<p class="mt-1 text-sm text-red-600">{errors.yellowThreshold[0]}</p>
						{/if}
					</div>
				</div>
			</div>

			<div class="surface-bg shadow rounded-lg p-6 space-y-6">
				<h2 class="text-lg font-semibold text-primary border-b border-default pb-2">
					Beschäftigungsstatus-Gewichtungen
				</h2>

				<div class="grid grid-cols-1 md:grid-cols-3 gap-4">
					<div>
						<label for="employedBonus" class="block text-sm font-medium text-primary">
							Angestellt (Bonus-Punkte)
						</label>
						<input
							type="number"
							id="employedBonus"
							name="employedBonus"
							value={config.employedBonus}
							min="0"
							max="100"
							class="mt-1 block w-full rounded-md border-default shadow-sm focus:border-brand focus:ring-brand sm:text-sm"
							data-testid="input-employed-bonus"
						/>
						{#if errors?.employedBonus}
							<p class="mt-1 text-sm text-red-600">{errors.employedBonus[0]}</p>
						{/if}
					</div>

					<div>
						<label for="selfEmployedBonus" class="block text-sm font-medium text-primary">
							Selbstständig (Abzug-Punkte)
						</label>
						<input
							type="number"
							id="selfEmployedBonus"
							name="selfEmployedBonus"
							value={config.selfEmployedBonus}
							min="0"
							max="100"
							class="mt-1 block w-full rounded-md border-default shadow-sm focus:border-brand focus:ring-brand sm:text-sm"
							data-testid="input-self-employed-bonus"
						/>
						{#if errors?.selfEmployedBonus}
							<p class="mt-1 text-sm text-red-600">{errors.selfEmployedBonus[0]}</p>
						{/if}
					</div>

					<div>
						<label for="unemployedPenalty" class="block text-sm font-medium text-primary">
							Arbeitslos (Abzug-Punkte)
						</label>
						<input
							type="number"
							id="unemployedPenalty"
							name="unemployedPenalty"
							value={config.unemployedPenalty}
							min="0"
							max="100"
							class="mt-1 block w-full rounded-md border-default shadow-sm focus:border-brand focus:ring-brand sm:text-sm"
							data-testid="input-unemployed-penalty"
						/>
						{#if errors?.unemployedPenalty}
							<p class="mt-1 text-sm text-red-600">{errors.unemployedPenalty[0]}</p>
						{/if}
					</div>
				</div>
			</div>

			<div class="surface-bg shadow rounded-lg p-6 space-y-6">
				<h2 class="text-lg font-semibold text-primary border-b border-default pb-2">
					Zahlungsverzug
				</h2>

				<div>
					<label for="paymentDefaultPenalty" class="block text-sm font-medium text-primary">
						Zahlungsverzug (Abzug-Punkte)
					</label>
					<input
						type="number"
						id="paymentDefaultPenalty"
						name="paymentDefaultPenalty"
						value={config.paymentDefaultPenalty}
						min="0"
						max="100"
						class="mt-1 block w-full max-w-xs rounded-md border-default shadow-sm focus:border-brand focus:ring-brand sm:text-sm"
						data-testid="input-payment-default-penalty"
					/>
					{#if errors?.paymentDefaultPenalty}
						<p class="mt-1 text-sm text-red-600">{errors.paymentDefaultPenalty[0]}</p>
					{/if}
				</div>
			</div>

			<div class="flex justify-end">
				<button
					type="submit"
					class="btn-primary px-6 py-2"
					data-testid="btn-save-config"
				>
					Konfiguration speichern
				</button>
			</div>
		</form>

		{#if config.updatedAt && config.updatedBy !== 'system'}
			<div class="text-sm text-secondary">
				Zuletzt aktualisiert: {new Date(config.updatedAt).toLocaleString('de-DE')} von {config.updatedBy}
			</div>
		{/if}
	</div>
</RoleGuard>
