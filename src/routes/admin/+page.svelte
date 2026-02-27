<script lang="ts">
	let { data } = $props();

	let thresholdGreen = $state(data.config.thresholdGreen);
	let thresholdYellow = $state(data.config.thresholdYellow);
	let weightIncome = $state(data.config.weightIncome);
	let weightEmployment = $state(data.config.weightEmployment);
	let weightPaymentDefault = $state(data.config.weightPaymentDefault);
	let updatedAt = $state(data.config.updatedAt);
	let updatedBy = $state(data.config.updatedBy);

	let saving = $state(false);
	let successMessage = $state('');
	let errorMessage = $state('');

	async function handleSave() {
		saving = true;
		successMessage = '';
		errorMessage = '';

		try {
			const response = await fetch('/api/admin/scoring-config', {
				method: 'PUT',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({
					thresholdGreen,
					thresholdYellow,
					weightIncome,
					weightEmployment,
					weightPaymentDefault
				})
			});

			if (!response.ok) {
				const err = await response.json();
				throw new Error(err.message || 'Fehler beim Speichern');
			}

			const updated = await response.json();
			thresholdGreen = updated.thresholdGreen;
			thresholdYellow = updated.thresholdYellow;
			weightIncome = updated.weightIncome;
			weightEmployment = updated.weightEmployment;
			weightPaymentDefault = updated.weightPaymentDefault;
			updatedAt = updated.updatedAt;
			updatedBy = updated.updatedBy;
			successMessage = 'Konfiguration erfolgreich gespeichert.';
		} catch (err) {
			errorMessage = err instanceof Error ? err.message : 'Unbekannter Fehler';
		} finally {
			saving = false;
		}
	}
</script>

<div class="space-y-6">
	<div>
		<h1 class="text-2xl font-bold text-primary">Scoring-Konfiguration</h1>
		<p class="mt-1 text-sm text-secondary">
			Passen Sie die Schwellenwerte und Gewichtungen der Scoring-Logik an. Änderungen wirken sich nur auf neue Anträge aus.
		</p>
	</div>

	{#if successMessage}
		<div class="rounded-md bg-green-50 p-4 border border-green-200">
			<p class="text-sm text-green-800">{successMessage}</p>
		</div>
	{/if}

	{#if errorMessage}
		<div class="rounded-md bg-red-50 p-4 border border-red-200">
			<p class="text-sm text-red-800">{errorMessage}</p>
		</div>
	{/if}

	<div class="surface-bg shadow rounded-lg p-6 border border-default">
		<form
			onsubmit={(e) => {
				e.preventDefault();
				handleSave();
			}}
			class="space-y-6"
		>
			<div>
				<h2 class="text-lg font-semibold text-primary mb-4">Ampel-Schwellenwerte</h2>
				<div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
					<div>
						<label for="thresholdGreen" class="block text-sm font-medium text-primary">
							Schwellenwert Grün (Score &ge;)
						</label>
						<input
							id="thresholdGreen"
							type="number"
							min="0"
							max="100"
							bind:value={thresholdGreen}
							class="mt-1 block w-full rounded-md border border-default px-3 py-2 text-sm shadow-sm focus:border-brand focus:ring-1 focus:ring-brand"
						/>
						<p class="mt-1 text-xs text-secondary">Standard: 75</p>
					</div>
					<div>
						<label for="thresholdYellow" class="block text-sm font-medium text-primary">
							Schwellenwert Gelb (Score &ge;)
						</label>
						<input
							id="thresholdYellow"
							type="number"
							min="0"
							max="100"
							bind:value={thresholdYellow}
							class="mt-1 block w-full rounded-md border border-default px-3 py-2 text-sm shadow-sm focus:border-brand focus:ring-1 focus:ring-brand"
						/>
						<p class="mt-1 text-xs text-secondary">Standard: 50</p>
					</div>
				</div>
			</div>

			<div>
				<h2 class="text-lg font-semibold text-primary mb-4">Gewichtungen</h2>
				<div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
					<div>
						<label for="weightIncome" class="block text-sm font-medium text-primary">
							Einkommen
						</label>
						<input
							id="weightIncome"
							type="number"
							min="0"
							max="1"
							step="0.05"
							bind:value={weightIncome}
							class="mt-1 block w-full rounded-md border border-default px-3 py-2 text-sm shadow-sm focus:border-brand focus:ring-1 focus:ring-brand"
						/>
						<p class="mt-1 text-xs text-secondary">Standard: 0.4</p>
					</div>
					<div>
						<label for="weightEmployment" class="block text-sm font-medium text-primary">
							Beschäftigungsstatus
						</label>
						<input
							id="weightEmployment"
							type="number"
							min="0"
							max="1"
							step="0.05"
							bind:value={weightEmployment}
							class="mt-1 block w-full rounded-md border border-default px-3 py-2 text-sm shadow-sm focus:border-brand focus:ring-1 focus:ring-brand"
						/>
						<p class="mt-1 text-xs text-secondary">Standard: 0.3</p>
					</div>
					<div>
						<label for="weightPaymentDefault" class="block text-sm font-medium text-primary">
							Zahlungsverzug
						</label>
						<input
							id="weightPaymentDefault"
							type="number"
							min="0"
							max="1"
							step="0.05"
							bind:value={weightPaymentDefault}
							class="mt-1 block w-full rounded-md border border-default px-3 py-2 text-sm shadow-sm focus:border-brand focus:ring-1 focus:ring-brand"
						/>
						<p class="mt-1 text-xs text-secondary">Standard: 0.3</p>
					</div>
				</div>
			</div>

			<div class="border-t border-default pt-4">
				<div class="flex items-center justify-between">
					<div class="text-sm text-secondary">
						<p>Zuletzt geändert: {updatedAt ? new Date(updatedAt).toLocaleString('de-DE') : '-'}</p>
						<p>Geändert von: {updatedBy || '-'}</p>
					</div>
					<button
						type="submit"
						disabled={saving}
						class="btn-primary inline-flex items-center px-4 py-2"
					>
						{saving ? 'Speichern...' : 'Speichern'}
					</button>
				</div>
			</div>
		</form>
	</div>
</div>
