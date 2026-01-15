<script lang="ts">
	import { isApplicant, isProcessor } from '$lib/stores/role';
	import { FileText, ClipboardCheck, ArrowRight, Shield, TrendingUp, Users } from 'lucide-svelte';
</script>

<svelte:head>
	<title>Risikomanagement-Plattform</title>
</svelte:head>

<div class="space-y-8">
	<div class="text-center">
		<h1 class="text-4xl font-bold text-gray-900 mb-4">
			Willkommen zur Risikomanagement-Plattform
		</h1>
		<p class="text-xl text-gray-600 max-w-2xl mx-auto">
			Ihre zentrale Anlaufstelle für die Beantragung und Bearbeitung von Kreditanträgen mit automatischer Risikobewertung.
		</p>
	</div>

	<div class="grid grid-cols-1 md:grid-cols-3 gap-6 mt-12">
		<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
			<div class="w-12 h-12 bg-indigo-100 rounded-lg flex items-center justify-center mb-4">
				<Shield class="w-6 h-6 text-indigo-600" />
			</div>
			<h3 class="text-lg font-semibold text-gray-900 mb-2">Automatische Bewertung</h3>
			<p class="text-gray-600">
				Jeder Antrag wird automatisch bewertet und erhält einen Score von 0-100 Punkten mit Ampelstatus.
			</p>
		</div>

		<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
			<div class="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center mb-4">
				<TrendingUp class="w-6 h-6 text-green-600" />
			</div>
			<h3 class="text-lg font-semibold text-gray-900 mb-2">Transparente Entscheidungen</h3>
			<p class="text-gray-600">
				Nachvollziehbare Bewertungsgründe helfen Ihnen, die Entscheidung zu verstehen.
			</p>
		</div>

		<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
			<div class="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center mb-4">
				<Users class="w-6 h-6 text-blue-600" />
			</div>
			<h3 class="text-lg font-semibold text-gray-900 mb-2">Einfache Verwaltung</h3>
			<p class="text-gray-600">
				Behalten Sie den Überblick über alle Ihre Anträge und deren aktuellen Status.
			</p>
		</div>
	</div>

	<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-8 mt-8">
		{#if $isApplicant}
			<h2 class="text-2xl font-bold text-gray-900 mb-4">Als Antragsteller</h2>
			<p class="text-gray-600 mb-6">
				Erstellen Sie neue Kreditanträge, speichern Sie Entwürfe und verfolgen Sie den Status Ihrer eingereichten Anträge.
			</p>
			<div class="flex flex-wrap gap-4">
				<a
					href="/applications/new"
					class="inline-flex items-center px-6 py-3 bg-indigo-600 text-white font-medium rounded-lg hover:bg-indigo-700 transition-colors"
				>
					<FileText class="w-5 h-5 mr-2" />
					Neuen Antrag erstellen
					<ArrowRight class="w-5 h-5 ml-2" />
				</a>
				<a
					href="/applications"
					class="inline-flex items-center px-6 py-3 bg-white text-gray-700 font-medium rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
				>
					Meine Anträge ansehen
				</a>
			</div>
		{:else}
			<h2 class="text-2xl font-bold text-gray-900 mb-4">Als Antragsbearbeiter</h2>
			<p class="text-gray-600 mb-6">
				Prüfen Sie eingereichte Anträge, treffen Sie fundierte Entscheidungen und verfolgen Sie die Bearbeitungshistorie.
			</p>
			<a
				href="/processor"
				class="inline-flex items-center px-6 py-3 bg-indigo-600 text-white font-medium rounded-lg hover:bg-indigo-700 transition-colors"
			>
				<ClipboardCheck class="w-5 h-5 mr-2" />
				Anträge bearbeiten
				<ArrowRight class="w-5 h-5 ml-2" />
			</a>
		{/if}
	</div>
</div>
