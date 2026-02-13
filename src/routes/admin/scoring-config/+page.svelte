<script lang="ts">
	let { data } = $props();

	const config = $derived(data.config);

	let trafficLightGreen = $state(0);
	let trafficLightYellow = $state(0);
	let incomeExcellent = $state(0);
	let incomeGood = $state(0);
	let incomeModerate = $state(0);
	let affordComfortable = $state(0);
	let affordModerate = $state(0);
	let affordStretched = $state(0);
	let empEmployed = $state(0);
	let empSelfEmployed = $state(0);
	let empRetired = $state(0);
	let empUnemployed = $state(0);
	let paymentDefaultDeduction = $state(0);
	let initialized = $state(false);

	$effect(() => {
		if (!initialized) {
			trafficLightGreen = config.trafficLightGreen;
			trafficLightYellow = config.trafficLightYellow;
			incomeExcellent = config.incomeRatioThresholds.excellent;
			incomeGood = config.incomeRatioThresholds.good;
			incomeModerate = config.incomeRatioThresholds.moderate;
			affordComfortable = config.affordabilityThresholds.comfortable;
			affordModerate = config.affordabilityThresholds.moderate;
			affordStretched = config.affordabilityThresholds.stretched;
			empEmployed = config.employmentDeductions.employed;
			empSelfEmployed = config.employmentDeductions.self_employed;
			empRetired = config.employmentDeductions.retired;
			empUnemployed = config.employmentDeductions.unemployed;
			paymentDefaultDeduction = config.paymentDefaultDeduction;
			initialized = true;
		}
	});

	let errorMessage = $state('');
	let successMessage = $state('');
	let isSaving = $state(false);

	async function handleSave() {
		errorMessage = '';
		successMessage = '';
		isSaving = true;

		const payload = {
			trafficLightGreen,
			trafficLightYellow,
			incomeRatioThresholds: {
				excellent: incomeExcellent,
				good: incomeGood,
				moderate: incomeModerate
			},
			affordabilityThresholds: {
				comfortable: affordComfortable,
				moderate: affordModerate,
				stretched: affordStretched
			},
			employmentDeductions: {
				employed: empEmployed,
				self_employed: empSelfEmployed,
				retired: empRetired,
				unemployed: empUnemployed
			},
			paymentDefaultDeduction
		};

		try {
			const response = await fetch('/api/admin/scoring-config', {
				method: 'PUT',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(payload)
			});

			if (!response.ok) {
					const errorData = await response.json();
					if (errorData.details && Array.isArray(errorData.details)) {
						errorMessage = errorData.details.map((d: { message: string }) => d.message).join(', ');
					} else if (errorData.error) {
						errorMessage = errorData.error;
				} else {
					errorMessage = 'Fehler beim Speichern der Konfiguration';
				}
				return;
			}

			successMessage = 'Konfiguration erfolgreich gespeichert';
		} catch {
			errorMessage = 'Netzwerkfehler beim Speichern der Konfiguration';
		} finally {
			isSaving = false;
		}
	}
</script>

<div class="max-w-3xl mx-auto">
	<h1 class="text-2xl font-bold text-primary mb-6">Scoring-Konfiguration</h1>

	{#if errorMessage}
		<div class="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700" data-testid="scoring-config-error">
			{errorMessage}
		</div>
	{/if}

	{#if successMessage}
		<div class="mb-4 p-4 bg-green-50 border border-green-200 rounded-lg text-green-700" data-testid="scoring-config-success">
			{successMessage}
		</div>
	{/if}

	<form
		data-testid="admin-scoring-form"
		onsubmit={(e) => { e.preventDefault(); handleSave(); }}
		class="space-y-8"
	>
		<section class="surface-bg border border-default rounded-lg p-6">
			<h2 class="text-lg font-semibold text-primary mb-4">Ampel-Schwellenwerte</h2>
			<p class="text-sm text-secondary mb-4">Bestimmt die Farbe der Ampel basierend auf dem Score.</p>
			<div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
				<div>
					<label for="traffic-light-green" class="block text-sm font-medium text-primary mb-1">
						Grün-Schwelle (Score ≥)
					</label>
					<input
						id="traffic-light-green"
						data-testid="input-traffic-light-green"
						type="number"
						min="0"
						max="100"
						bind:value={trafficLightGreen}
						class="input-field w-full"
					/>
				</div>
				<div>
					<label for="traffic-light-yellow" class="block text-sm font-medium text-primary mb-1">
						Gelb-Schwelle (Score ≥)
					</label>
					<input
						id="traffic-light-yellow"
						data-testid="input-traffic-light-yellow"
						type="number"
						min="0"
						max="100"
						bind:value={trafficLightYellow}
						class="input-field w-full"
					/>
				</div>
			</div>
		</section>

		<section class="surface-bg border border-default rounded-lg p-6">
			<h2 class="text-lg font-semibold text-primary mb-4">Einkommens-/Fixkosten-Ratio</h2>
			<p class="text-sm text-secondary mb-4">Schwellenwerte für das Verhältnis von verfügbarem Einkommen zu Gesamteinkommen (0-1).</p>
			<div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
				<div>
					<label for="income-excellent" class="block text-sm font-medium text-primary mb-1">
						Exzellent (≥)
					</label>
					<input
						id="income-excellent"
						data-testid="input-income-excellent"
						type="number"
						min="0"
						max="1"
						step="0.01"
						bind:value={incomeExcellent}
						class="input-field w-full"
					/>
				</div>
				<div>
					<label for="income-good" class="block text-sm font-medium text-primary mb-1">
						Gut (≥)
					</label>
					<input
						id="income-good"
						data-testid="input-income-good"
						type="number"
						min="0"
						max="1"
						step="0.01"
						bind:value={incomeGood}
						class="input-field w-full"
					/>
				</div>
				<div>
					<label for="income-moderate" class="block text-sm font-medium text-primary mb-1">
						Moderat (≥)
					</label>
					<input
						id="income-moderate"
						data-testid="input-income-moderate"
						type="number"
						min="0"
						max="1"
						step="0.01"
						bind:value={incomeModerate}
						class="input-field w-full"
					/>
				</div>
			</div>
		</section>

		<section class="surface-bg border border-default rounded-lg p-6">
			<h2 class="text-lg font-semibold text-primary mb-4">Raten-Tragbarkeit</h2>
			<p class="text-sm text-secondary mb-4">Schwellenwerte für das Verhältnis der Rate zum verfügbaren Einkommen (0-1).</p>
			<div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
				<div>
					<label for="afford-comfortable" class="block text-sm font-medium text-primary mb-1">
						Komfortabel (≤)
					</label>
					<input
						id="afford-comfortable"
						data-testid="input-afford-comfortable"
						type="number"
						min="0"
						max="1"
						step="0.01"
						bind:value={affordComfortable}
						class="input-field w-full"
					/>
				</div>
				<div>
					<label for="afford-moderate" class="block text-sm font-medium text-primary mb-1">
						Moderat (≤)
					</label>
					<input
						id="afford-moderate"
						data-testid="input-afford-moderate"
						type="number"
						min="0"
						max="1"
						step="0.01"
						bind:value={affordModerate}
						class="input-field w-full"
					/>
				</div>
				<div>
					<label for="afford-stretched" class="block text-sm font-medium text-primary mb-1">
						Belastet (≤)
					</label>
					<input
						id="afford-stretched"
						data-testid="input-afford-stretched"
						type="number"
						min="0"
						max="1"
						step="0.01"
						bind:value={affordStretched}
						class="input-field w-full"
					/>
				</div>
			</div>
		</section>

		<section class="surface-bg border border-default rounded-lg p-6">
			<h2 class="text-lg font-semibold text-primary mb-4">Beschäftigungsstatus-Abzüge</h2>
			<p class="text-sm text-secondary mb-4">Punktabzüge basierend auf dem Beschäftigungsstatus (0-100).</p>
			<div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
				<div>
					<label for="emp-employed" class="block text-sm font-medium text-primary mb-1">
						Angestellt
					</label>
					<input
						id="emp-employed"
						data-testid="input-emp-employed"
						type="number"
						min="0"
						max="100"
						bind:value={empEmployed}
						class="input-field w-full"
					/>
				</div>
				<div>
					<label for="emp-self-employed" class="block text-sm font-medium text-primary mb-1">
						Selbstständig
					</label>
					<input
						id="emp-self-employed"
						data-testid="input-emp-self-employed"
						type="number"
						min="0"
						max="100"
						bind:value={empSelfEmployed}
						class="input-field w-full"
					/>
				</div>
				<div>
					<label for="emp-retired" class="block text-sm font-medium text-primary mb-1">
						Ruhestand
					</label>
					<input
						id="emp-retired"
						data-testid="input-emp-retired"
						type="number"
						min="0"
						max="100"
						bind:value={empRetired}
						class="input-field w-full"
					/>
				</div>
				<div>
					<label for="emp-unemployed" class="block text-sm font-medium text-primary mb-1">
						Arbeitslos
					</label>
					<input
						id="emp-unemployed"
						data-testid="input-emp-unemployed"
						type="number"
						min="0"
						max="100"
						bind:value={empUnemployed}
						class="input-field w-full"
					/>
				</div>
			</div>
		</section>

		<section class="surface-bg border border-default rounded-lg p-6">
			<h2 class="text-lg font-semibold text-primary mb-4">Zahlungsausfall-Abzug</h2>
			<p class="text-sm text-secondary mb-4">Punktabzug bei früheren Zahlungsausfällen (0-100).</p>
			<div class="max-w-xs">
				<label for="payment-default" class="block text-sm font-medium text-primary mb-1">
					Abzug bei Zahlungsausfall
				</label>
				<input
					id="payment-default"
					data-testid="input-payment-default-deduction"
					type="number"
					min="0"
					max="100"
					bind:value={paymentDefaultDeduction}
					class="input-field w-full"
				/>
			</div>
		</section>

		<div class="flex justify-end">
			<button
				type="submit"
				data-testid="btn-save-scoring-config"
				class="btn-primary px-6 py-2"
				disabled={isSaving}
			>
				{isSaving ? 'Speichern...' : 'Konfiguration speichern'}
			</button>
		</div>
	</form>
</div>
