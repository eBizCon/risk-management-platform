<script lang="ts">
	import '../app.css';
	import { currentUser, isApplicant, isProcessor } from '$lib/stores/role';
	import RoleSwitcher from '$lib/components/RoleSwitcher.svelte';
	import { FileText, ClipboardList, Home } from 'lucide-svelte';

	let { children } = $props();
</script>

<div class="min-h-screen page-bg">
	<nav class="shadow-sm surface-bg border-b border-default">
		<div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
			<div class="flex justify-between h-16">
				<div class="flex">
					<div class="flex-shrink-0 flex items-center">
						<a href="/" class="text-xl font-bold text-brand">
							Risikomanagement
						</a>
					</div>
					<div class="hidden sm:ml-8 sm:flex sm:space-x-4">
						<a
							href="/"
							class="nav-link inline-flex items-center px-3 py-2 text-sm font-medium rounded-md"
						>
							<Home class="w-4 h-4 mr-2" />
							Startseite
						</a>
						{#if $isApplicant}
							<a
								href="/applications"
								class="nav-link inline-flex items-center px-3 py-2 text-sm font-medium rounded-md"
							>
								<FileText class="w-4 h-4 mr-2" />
								Meine Anträge
							</a>
							<a
								href="/applications/new"
								class="nav-link inline-flex items-center px-3 py-2 text-sm font-medium rounded-md"
							>
								<ClipboardList class="w-4 h-4 mr-2" />
								Neuer Antrag
							</a>
						{/if}
						{#if $isProcessor}
							<a
								href="/processor"
								class="nav-link inline-flex items-center px-3 py-2 text-sm font-medium rounded-md"
							>
								<ClipboardList class="w-4 h-4 mr-2" />
								Anträge bearbeiten
							</a>
						{/if}
					</div>
				</div>
				<div class="flex items-center">
					<div class="text-sm text-secondary">
						<span class="font-medium text-primary">{$currentUser.name}</span>
						<span class="mx-1">|</span>
						<span>{$isApplicant ? 'Antragsteller' : 'Antragsbearbeiter'}</span>
					</div>
				</div>
			</div>
		</div>
	</nav>

	<main class="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
		{@render children()}
	</main>

	<RoleSwitcher />
</div>
