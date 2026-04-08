<script lang="ts">
	import { untrack } from 'svelte';
	import RoleGuard from '$lib/components/RoleGuard.svelte';
	import { Settings, Save, RefreshCw, AlertTriangle } from 'lucide-svelte';
	import type { PageData } from './$types';
	import type { ScoringConfig } from './+page.ts';

	let { data }: { data: PageData } = $props();

	const initialConfig = untrack(() => data.config);

	let config = $state<ScoringConfig | null>(initialConfig ?? null);
	let saving = $state(false);
	let rescoring = $state(false);
	let successMessage = $state('');
	let errorMessage = $state('');
	let rescoreResult = $state('');

	let greenThreshold = $state(initialConfig?.greenThreshold ?? 75);
	let yellowThreshold = $state(initialConfig?.yellowThreshold ?? 50);
	let incomeRatioGood = $state(initialConfig?.incomeRatioGood ?? 0.5);
	let incomeRatioModerate = $state(initialConfig?.incomeRatioModerate ?? 0.35);
	let incomeRatioLimited = $state(initialConfig?.incomeRatioLimited ?? 0.2);
	let penaltyModerateRatio = $state(initialConfig?.penaltyModerateRatio ?? 10);
	let penaltyLimitedRatio = $state(initialConfig?.penaltyLimitedRatio ?? 25);
	let penaltyCriticalRatio = $state(initialConfig?.penaltyCriticalRatio ?? 40);
	let rateGood = $state(initialConfig?.rateGood ?? 0.3);
	let rateModerate = $state(initialConfig?.rateModerate ?? 0.5);
	let rateHeavy = $state(initialConfig?.rateHeavy ?? 0.7);
	let penaltyModerateRate = $state(initialConfig?.penaltyModerateRate ?? 10);
	let penaltyHeavyRate = $state(initialConfig?.penaltyHeavyRate ?? 20);
	let penaltyExcessiveRate = $state(initialConfig?.penaltyExcessiveRate ?? 35);
	let penaltySelfEmployed = $state(initialConfig?.penaltySelfEmployed ?? 10);
	let penaltyRetired = $state(initialConfig?.penaltyRetired ?? 5);
	let penaltyUnemployed = $state(initialConfig?.penaltyUnemployed ?? 35);
	let penaltyPaymentDefault = $state(initialConfig?.penaltyPaymentDefault ?? 25);

	function clearMessages() {
		successMessage = '';
		errorMessage = '';
		rescoreResult = '';
	}

	async function handleSave() {
		clearMessages();
		saving = true;
		try {
			const res = await fetch('/api/scoring-config', {
				method: 'PUT',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({
					greenThreshold,
					yellowThreshold,
					incomeRatioGood,
					incomeRatioModerate,
					incomeRatioLimited,
					penaltyModerateRatio,
					penaltyLimitedRatio,
					penaltyCriticalRatio,
					rateGood,
					rateModerate,
					rateHeavy,
					penaltyModerateRate,
					penaltyHeavyRate,
					penaltyExcessiveRate,
					penaltySelfEmployed,
					penaltyRetired,
					penaltyUnemployed,
					penaltyPaymentDefault
				})
			});

			if (res.ok) {
				config = await res.json();
				successMessage = `Konfiguration gespeichert (Version ${config!.version})`;
			} else {
				const err = await res.json().catch(() => ({ message: 'Fehler beim Speichern' }));
				errorMessage = err.message ?? 'Fehler beim Speichern';
			}
		} catch {
			errorMessage = 'Netzwerkfehler beim Speichern';
		} finally {
			saving = false;
		}
	}

	async function handleRescore() {
		clearMessages();
		rescoring = true;
		try {
			const res = await fetch('/api/scoring-config/rescore', { method: 'POST' });

			if (res.ok) {
				const result = await res.json();
				rescoreResult = `${result.rescoredCount} Anträge wurden neu bewertet.`;
			} else {
				const err = await res.json().catch(() => ({ message: 'Fehler beim Rescoring' }));
				errorMessage = err.message ?? 'Fehler beim Rescoring';
			}
		} catch {
			errorMessage = 'Netzwerkfehler beim Rescoring';
		} finally {
			rescoring = false;
		}
	}
</script>

<svelte:head>
	<title>Scoring-Konfiguration - Risikomanagement</title>
</svelte:head>

<RoleGuard requiredRole="risk_manager">
	<div class="space-y-6" data-testid="scoring-config-page">
		<div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
			<div>
				<h1 class="text-2xl font-bold text-primary flex items-center gap-2">
					<Settings class="w-6 h-6" />
					Scoring-Konfiguration
				</h1>
				{#if config}
					<p class="text-sm text-secondary mt-1">
						Version {config.version} · Erstellt von {config.createdBy} · {new Date(
							config.createdAt
						).toLocaleString('de-DE')}
					</p>
				{/if}
			</div>
			<button
				type="button"
				class="btn-secondary inline-flex items-center gap-2 px-4 py-2"
				onclick={handleRescore}
				disabled={rescoring}
				data-testid="scoring-config-rescore"
			>
				<RefreshCw class="w-4 h-4 {rescoring ? 'animate-spin' : ''}" />
				{rescoring ? 'Bewertung läuft...' : 'Offene Anträge neu bewerten'}
			</button>
		</div>

		{#if successMessage}
			<div
				class="rounded-md bg-green-50 border border-green-200 p-4"
				data-testid="scoring-config-success"
			>
				<p class="text-sm text-green-800">{successMessage}</p>
			</div>
		{/if}
		{#if errorMessage}
			<div
				class="rounded-md bg-red-50 border border-red-200 p-4"
				data-testid="scoring-config-error"
			>
				<p class="text-sm text-red-800">{errorMessage}</p>
			</div>
		{/if}
		{#if rescoreResult}
			<div
				class="rounded-md bg-blue-50 border border-blue-200 p-4"
				data-testid="scoring-config-rescore-result"
			>
				<p class="text-sm text-blue-800">{rescoreResult}</p>
			</div>
		{/if}

		<form
			onsubmit={(e) => {
				e.preventDefault();
				handleSave();
			}}
			class="space-y-8"
		>
			<div class="card rounded-lg shadow-sm p-6 space-y-6">
				<h2 class="text-lg font-semibold text-primary">Ampel-Schwellenwerte</h2>
				<p class="text-sm text-secondary">
					Legen Sie fest, ab welchem Score die Ampel grün bzw. gelb anzeigt.
				</p>
				<div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
					<div>
						<label for="greenThreshold" class="block text-sm font-medium text-primary mb-1"
							>Grün ab (Score ≥)</label
						>
						<input
							id="greenThreshold"
							type="number"
							min="0"
							max="100"
							bind:value={greenThreshold}
							class="input w-full"
							data-testid="scoring-config-green-threshold"
						/>
					</div>
					<div>
						<label for="yellowThreshold" class="block text-sm font-medium text-primary mb-1"
							>Gelb ab (Score ≥)</label
						>
						<input
							id="yellowThreshold"
							type="number"
							min="0"
							max="100"
							bind:value={yellowThreshold}
							class="input w-full"
							data-testid="scoring-config-yellow-threshold"
						/>
					</div>
				</div>
			</div>

			<div class="card rounded-lg shadow-sm p-6 space-y-6">
				<h2 class="text-lg font-semibold text-primary">Einkommens-/Fixkosten-Verhältnis</h2>
				<p class="text-sm text-secondary">
					Schwellen für die Bewertung des Verhältnisses zwischen Einkommen und Fixkosten, sowie
					zugehörige Abzüge.
				</p>
				<div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
					<div>
						<label for="incomeRatioGood" class="block text-sm font-medium text-primary mb-1"
							>Gut (Ratio ≥)</label
						>
						<input
							id="incomeRatioGood"
							type="number"
							step="0.01"
							min="0"
							max="1"
							bind:value={incomeRatioGood}
							class="input w-full"
							data-testid="scoring-config-income-ratio-good"
						/>
					</div>
					<div>
						<label for="incomeRatioModerate" class="block text-sm font-medium text-primary mb-1"
							>Moderat (Ratio ≥)</label
						>
						<input
							id="incomeRatioModerate"
							type="number"
							step="0.01"
							min="0"
							max="1"
							bind:value={incomeRatioModerate}
							class="input w-full"
							data-testid="scoring-config-income-ratio-moderate"
						/>
					</div>
					<div>
						<label for="incomeRatioLimited" class="block text-sm font-medium text-primary mb-1"
							>Eingeschränkt (Ratio ≥)</label
						>
						<input
							id="incomeRatioLimited"
							type="number"
							step="0.01"
							min="0"
							max="1"
							bind:value={incomeRatioLimited}
							class="input w-full"
							data-testid="scoring-config-income-ratio-limited"
						/>
					</div>
				</div>
				<div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
					<div>
						<label for="penaltyModerateRatio" class="block text-sm font-medium text-primary mb-1"
							>Abzug moderat</label
						>
						<input
							id="penaltyModerateRatio"
							type="number"
							min="0"
							max="100"
							bind:value={penaltyModerateRatio}
							class="input w-full"
							data-testid="scoring-config-penalty-moderate-ratio"
						/>
					</div>
					<div>
						<label for="penaltyLimitedRatio" class="block text-sm font-medium text-primary mb-1"
							>Abzug eingeschränkt</label
						>
						<input
							id="penaltyLimitedRatio"
							type="number"
							min="0"
							max="100"
							bind:value={penaltyLimitedRatio}
							class="input w-full"
							data-testid="scoring-config-penalty-limited-ratio"
						/>
					</div>
					<div>
						<label for="penaltyCriticalRatio" class="block text-sm font-medium text-primary mb-1"
							>Abzug kritisch</label
						>
						<input
							id="penaltyCriticalRatio"
							type="number"
							min="0"
							max="100"
							bind:value={penaltyCriticalRatio}
							class="input w-full"
							data-testid="scoring-config-penalty-critical-ratio"
						/>
					</div>
				</div>
			</div>

			<div class="card rounded-lg shadow-sm p-6 space-y-6">
				<h2 class="text-lg font-semibold text-primary">Raten-Tragbarkeit</h2>
				<p class="text-sm text-secondary">
					Schwellen für das Verhältnis Rate/verfügbares Einkommen und zugehörige Abzüge.
				</p>
				<div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
					<div>
						<label for="rateGood" class="block text-sm font-medium text-primary mb-1"
							>Gut tragbar (≤)</label
						>
						<input
							id="rateGood"
							type="number"
							step="0.01"
							min="0"
							max="1"
							bind:value={rateGood}
							class="input w-full"
							data-testid="scoring-config-rate-good"
						/>
					</div>
					<div>
						<label for="rateModerate" class="block text-sm font-medium text-primary mb-1"
							>Moderat tragbar (≤)</label
						>
						<input
							id="rateModerate"
							type="number"
							step="0.01"
							min="0"
							max="1"
							bind:value={rateModerate}
							class="input w-full"
							data-testid="scoring-config-rate-moderate"
						/>
					</div>
					<div>
						<label for="rateHeavy" class="block text-sm font-medium text-primary mb-1"
							>Belastend (≤)</label
						>
						<input
							id="rateHeavy"
							type="number"
							step="0.01"
							min="0"
							max="1"
							bind:value={rateHeavy}
							class="input w-full"
							data-testid="scoring-config-rate-heavy"
						/>
					</div>
				</div>
				<div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
					<div>
						<label for="penaltyModerateRate" class="block text-sm font-medium text-primary mb-1"
							>Abzug moderat</label
						>
						<input
							id="penaltyModerateRate"
							type="number"
							min="0"
							max="100"
							bind:value={penaltyModerateRate}
							class="input w-full"
							data-testid="scoring-config-penalty-moderate-rate"
						/>
					</div>
					<div>
						<label for="penaltyHeavyRate" class="block text-sm font-medium text-primary mb-1"
							>Abzug belastend</label
						>
						<input
							id="penaltyHeavyRate"
							type="number"
							min="0"
							max="100"
							bind:value={penaltyHeavyRate}
							class="input w-full"
							data-testid="scoring-config-penalty-heavy-rate"
						/>
					</div>
					<div>
						<label for="penaltyExcessiveRate" class="block text-sm font-medium text-primary mb-1"
							>Abzug übermäßig</label
						>
						<input
							id="penaltyExcessiveRate"
							type="number"
							min="0"
							max="100"
							bind:value={penaltyExcessiveRate}
							class="input w-full"
							data-testid="scoring-config-penalty-excessive-rate"
						/>
					</div>
				</div>
			</div>

			<div class="card rounded-lg shadow-sm p-6 space-y-6">
				<h2 class="text-lg font-semibold text-primary">Beschäftigungsstatus-Abzüge</h2>
				<p class="text-sm text-secondary">Punkteabzüge basierend auf dem Beschäftigungsstatus.</p>
				<div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
					<div>
						<label for="penaltySelfEmployed" class="block text-sm font-medium text-primary mb-1"
							>Selbständig</label
						>
						<input
							id="penaltySelfEmployed"
							type="number"
							min="0"
							max="100"
							bind:value={penaltySelfEmployed}
							class="input w-full"
							data-testid="scoring-config-penalty-self-employed"
						/>
					</div>
					<div>
						<label for="penaltyRetired" class="block text-sm font-medium text-primary mb-1"
							>Im Ruhestand</label
						>
						<input
							id="penaltyRetired"
							type="number"
							min="0"
							max="100"
							bind:value={penaltyRetired}
							class="input w-full"
							data-testid="scoring-config-penalty-retired"
						/>
					</div>
					<div>
						<label for="penaltyUnemployed" class="block text-sm font-medium text-primary mb-1"
							>Arbeitslos</label
						>
						<input
							id="penaltyUnemployed"
							type="number"
							min="0"
							max="100"
							bind:value={penaltyUnemployed}
							class="input w-full"
							data-testid="scoring-config-penalty-unemployed"
						/>
					</div>
				</div>
			</div>

			<div class="card rounded-lg shadow-sm p-6 space-y-6">
				<h2 class="text-lg font-semibold text-primary">Zahlungsverzug</h2>
				<div>
					<label for="penaltyPaymentDefault" class="block text-sm font-medium text-primary mb-1"
						>Abzug bei Zahlungsverzug</label
					>
					<input
						id="penaltyPaymentDefault"
						type="number"
						min="0"
						max="100"
						bind:value={penaltyPaymentDefault}
						class="input w-full max-w-xs"
						data-testid="scoring-config-penalty-payment-default"
					/>
				</div>
			</div>

			<div class="flex items-center gap-4">
				<button
					type="submit"
					class="btn-primary inline-flex items-center gap-2 px-6 py-2"
					disabled={saving}
					data-testid="scoring-config-save"
				>
					<Save class="w-4 h-4" />
					{saving ? 'Speichern...' : 'Konfiguration speichern'}
				</button>
				<div class="flex items-center gap-2 text-sm text-secondary">
					<AlertTriangle class="w-4 h-4" />
					<span
						>Änderungen erstellen eine neue Version. Bestehende Bewertungen bleiben unverändert.</span
					>
				</div>
			</div>
		</form>
	</div>
</RoleGuard>
