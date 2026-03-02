<script lang="ts">
	import RoleGuard from '$lib/components/RoleGuard.svelte';
	import { Settings, Save, RotateCcw } from 'lucide-svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	const initialConfig = $derived(data.config);
	let greenThreshold = $state(0);
	let yellowThreshold = $state(0);
	let weightIncome = $state(0);
	let weightFixedCosts = $state(0);
	let weightEmployment = $state(0);
	let weightPaymentDefault = $state(0);
	let initialized = $state(false);

	$effect(() => {
		if (!initialized) {
			greenThreshold = initialConfig.greenThreshold;
			yellowThreshold = initialConfig.yellowThreshold;
			weightIncome = initialConfig.weights.income;
			weightFixedCosts = initialConfig.weights.fixedCosts;
			weightEmployment = initialConfig.weights.employment;
			weightPaymentDefault = initialConfig.weights.paymentDefault;
			initialized = true;
		}
	});

	let saving = $state(false);
	let successMessage = $state('');
	let errorMessage = $state('');

	async function handleSave() {
		saving = true;
		successMessage = '';
		errorMessage = '';

		if (yellowThreshold >= greenThreshold) {
			errorMessage = 'Der Gelb-Schwellenwert muss kleiner als der Grün-Schwellenwert sein.';
			saving = false;
			return;
		}

		try {
			const response = await fetch('/api/admin/scoring-config', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({
					greenThreshold,
					yellowThreshold,
					weights: {
						income: weightIncome,
						fixedCosts: weightFixedCosts,
						employment: weightEmployment,
						paymentDefault: weightPaymentDefault
					}
				})
			});

			if (!response.ok) {
				const text = await response.text();
				errorMessage = `Fehler beim Speichern: ${text}`;
				return;
			}

			successMessage = 'Konfiguration erfolgreich gespeichert.';
		} catch {
			errorMessage = 'Netzwerkfehler beim Speichern der Konfiguration.';
		} finally {
			saving = false;
		}
	}

	function handleReset() {
		greenThreshold = 75;
		yellowThreshold = 50;
		weightIncome = 1.0;
		weightFixedCosts = 1.0;
		weightEmployment = 1.0;
		weightPaymentDefault = 1.0;
		successMessage = '';
		errorMessage = '';
	}
</script>

<svelte:head>
	<title>Scoring konfigurieren - Risikomanagement</title>
</svelte:head>

<RoleGuard requiredRole="admin">
	<div class="space-y-6">
		<div>
			<h1 class="text-2xl font-bold text-primary" data-testid="admin-scoring-heading">
				<Settings class="w-6 h-6 inline-block mr-2" />
				Scoring konfigurieren
			</h1>
			<p class="text-secondary mt-1">
				Schwellenwerte und Gewichtungen für die automatische Kreditbewertung anpassen.
			</p>
		</div>

		{#if successMessage}
			<div class="rounded-md bg-green-50 border border-green-200 p-4" data-testid="success-message">
				<p class="text-sm text-green-800">{successMessage}</p>
			</div>
		{/if}

		{#if errorMessage}
			<div class="rounded-md bg-red-50 border border-red-200 p-4" data-testid="error-message">
				<p class="text-sm text-red-800">{errorMessage}</p>
			</div>
		{/if}

		<div class="card p-6 space-y-6">
			<div>
				<h2 class="text-lg font-semibold text-primary mb-4">Ampel-Schwellenwerte</h2>
				<p class="text-sm text-secondary mb-4">
					Bestimmen Sie die Score-Bereiche für die Ampelfarben. Der Score reicht von 0 bis 100.
				</p>
				<div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
					<div>
						<label for="greenThreshold" class="block text-sm font-medium text-primary mb-1">
							Grün-Schwellenwert (Score &ge;)
						</label>
						<input
							id="greenThreshold"
							type="number"
							min="0"
							max="100"
							bind:value={greenThreshold}
							class="w-full rounded-md border-default shadow-sm sm:text-sm"
							data-testid="input-green-threshold"
						/>
						<p class="text-xs text-secondary mt-1">Score &ge; {greenThreshold} = Grün (Positiv)</p>
					</div>
					<div>
						<label for="yellowThreshold" class="block text-sm font-medium text-primary mb-1">
							Gelb-Schwellenwert (Score &ge;)
						</label>
						<input
							id="yellowThreshold"
							type="number"
							min="0"
							max="100"
							bind:value={yellowThreshold}
							class="w-full rounded-md border-default shadow-sm sm:text-sm"
							data-testid="input-yellow-threshold"
						/>
						<p class="text-xs text-secondary mt-1">Score {yellowThreshold}–{greenThreshold - 1} = Gelb (Prüfung)</p>
					</div>
				</div>
				<p class="text-xs text-secondary mt-2">Score &lt; {yellowThreshold} = Rot (Kritisch)</p>
			</div>

			<hr class="border-default" />

			<div>
				<h2 class="text-lg font-semibold text-primary mb-4">Gewichtungen</h2>
				<p class="text-sm text-secondary mb-4">
					Passen Sie die Gewichtung der einzelnen Bewertungskriterien an. Standard ist 1.0 (keine Änderung).
				</p>
				<div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
					<div>
						<label for="weightIncome" class="block text-sm font-medium text-primary mb-1">
							Einkommen/Fixkosten-Verhältnis
						</label>
						<input
							id="weightIncome"
							type="number"
							min="0"
							max="5"
							step="0.1"
							bind:value={weightIncome}
							class="w-full rounded-md border-default shadow-sm sm:text-sm"
							data-testid="input-weight-income"
						/>
					</div>
					<div>
						<label for="weightFixedCosts" class="block text-sm font-medium text-primary mb-1">
							Ratenbelastung
						</label>
						<input
							id="weightFixedCosts"
							type="number"
							min="0"
							max="5"
							step="0.1"
							bind:value={weightFixedCosts}
							class="w-full rounded-md border-default shadow-sm sm:text-sm"
							data-testid="input-weight-fixed-costs"
						/>
					</div>
					<div>
						<label for="weightEmployment" class="block text-sm font-medium text-primary mb-1">
							Beschäftigungsstatus
						</label>
						<input
							id="weightEmployment"
							type="number"
							min="0"
							max="5"
							step="0.1"
							bind:value={weightEmployment}
							class="w-full rounded-md border-default shadow-sm sm:text-sm"
							data-testid="input-weight-employment"
						/>
					</div>
					<div>
						<label for="weightPaymentDefault" class="block text-sm font-medium text-primary mb-1">
							Zahlungsverzüge
						</label>
						<input
							id="weightPaymentDefault"
							type="number"
							min="0"
							max="5"
							step="0.1"
							bind:value={weightPaymentDefault}
							class="w-full rounded-md border-default shadow-sm sm:text-sm"
							data-testid="input-weight-payment-default"
						/>
					</div>
				</div>
			</div>

			<hr class="border-default" />

			<div class="flex flex-col sm:flex-row gap-3">
				<button
					type="button"
					class="btn-primary inline-flex items-center px-4 py-2"
					onclick={handleSave}
					disabled={saving}
					data-testid="btn-save-config"
				>
					<Save class="w-4 h-4 mr-2" />
					{saving ? 'Speichern...' : 'Konfiguration speichern'}
				</button>
				<button
					type="button"
					class="btn-secondary inline-flex items-center px-4 py-2"
					onclick={handleReset}
					data-testid="btn-reset-config"
				>
					<RotateCcw class="w-4 h-4 mr-2" />
					Auf Standardwerte zurücksetzen
				</button>
			</div>
		</div>
	</div>
</RoleGuard>
