<script lang="ts">
	import { page } from '$app/stores';
	import { FileText, ClipboardCheck, ArrowRight, Shield, TrendingUp, Users, LogIn } from 'lucide-svelte';

	const user = $derived($page.data.user ?? null);
	const isApplicant = $derived(user?.role === 'applicant');
	const isProcessor = $derived(user?.role === 'processor');
</script>

<svelte:head>
	<title>Risikomanagement-Plattform</title>
</svelte:head>

<div class="space-y-8">
	<div class="text-center">
		<h1 class="text-2xl sm:text-3xl md:text-4xl font-bold text-primary mb-4">
			Willkommen zur Risikomanagement-Plattform
		</h1>
		<p class="text-lg sm:text-xl text-secondary max-w-2xl mx-auto">
			Ihre zentrale Anlaufstelle für die Beantragung und Bearbeitung von Kreditanträgen mit automatischer Risikobewertung.
		</p>
	</div>

	<div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6 mt-12">
		<div class="card p-6">
			<div class="w-12 h-12 rounded-lg flex items-center justify-center mb-4 bg-brand-primary-soft text-brand-primary">
				<Shield class="w-6 h-6" />
			</div>
			<h3 class="text-lg font-semibold text-primary mb-2">Automatische Bewertung</h3>
			<p class="text-secondary">
				Jeder Antrag wird automatisch bewertet und erhält einen Score von 0-100 Punkten mit Ampelstatus.
			</p>
		</div>

		<div class="card p-6">
			<div class="w-12 h-12 rounded-lg flex items-center justify-center mb-4 bg-success/15 text-success">
				<TrendingUp class="w-6 h-6" />
			</div>
			<h3 class="text-lg font-semibold text-primary mb-2">Transparente Entscheidungen</h3>
			<p class="text-secondary">
				Nachvollziehbare Bewertungsgründe helfen Ihnen, die Entscheidung zu verstehen.
			</p>
		</div>

		<div class="card p-6">
			<div class="w-12 h-12 rounded-lg flex items-center justify-center mb-4 bg-info/15 text-info">
				<Users class="w-6 h-6" />
			</div>
			<h3 class="text-lg font-semibold text-primary mb-2">Einfache Verwaltung</h3>
			<p class="text-secondary">
				Behalten Sie den Überblick über alle Ihre Anträge und deren aktuellen Status.
			</p>
		</div>
	</div>

	<div class="card p-8 mt-8">
		{#if isApplicant}
			<h2 class="text-2xl font-bold text-primary mb-4">Als Antragsteller</h2>
			<p class="text-secondary mb-6">
				Erstellen Sie neue Kreditanträge, speichern Sie Entwürfe und verfolgen Sie den Status Ihrer eingereichten Anträge.
			</p>
			<div class="flex flex-col sm:flex-row gap-4">
				<a
					href="/applications/new"
					class="btn-primary inline-flex items-center px-6 py-3 w-full sm:w-auto"
				>
					<FileText class="w-5 h-5 mr-2" />
					Neuen Antrag erstellen
					<ArrowRight class="w-5 h-5 ml-2" />
				</a>
				<a
					href="/applications"
					class="btn-secondary inline-flex items-center px-6 py-3 w-full sm:w-auto"
				>
					Meine Anträge ansehen
				</a>
			</div>
		{:else if isProcessor}
			<h2 class="text-2xl font-bold text-primary mb-4">Als Antragsbearbeiter</h2>
			<p class="text-secondary mb-6">
				Prüfen Sie eingereichte Anträge, treffen Sie fundierte Entscheidungen und verfolgen Sie die Bearbeitungshistorie.
			</p>
			<div class="flex flex-col sm:flex-row gap-4">
				<a
					href="/processor"
					class="btn-primary inline-flex items-center px-6 py-3 w-full sm:w-auto"
				>
					<ClipboardCheck class="w-5 h-5 mr-2" />
					Anträge bearbeiten
					<ArrowRight class="w-5 h-5 ml-2" />
				</a>
			</div>
		{:else}
			<h2 class="text-2xl font-bold text-primary mb-4">Starten Sie jetzt</h2>
			<p class="text-secondary mb-6">
				Melden Sie sich an, um Anträge zu stellen oder zu bearbeiten.
			</p>
			<a href="/login" class="btn-primary inline-flex items-center px-6 py-3 w-full sm:w-auto" data-testid="hero-login">
				<LogIn class="w-5 h-5 mr-2" />
				Zum Login
				<ArrowRight class="w-5 h-5 ml-2" />
			</a>
		{/if}
	</div>
</div>
