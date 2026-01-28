<script lang="ts">
	import '../app.css';
	import { FileText, ClipboardList, Home, LogIn, LogOut } from 'lucide-svelte';

	let { data, children } = $props();

	const user = $derived(data.user ?? null);
	const isApplicant = $derived(user?.role === 'applicant');
	const isProcessor = $derived(user?.role === 'processor');
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
						{#if isApplicant}
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
						{#if isProcessor}
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
				<div class="flex items-center gap-4">
					{#if user}
						<div class="text-sm text-secondary text-right">
							<div class="font-medium text-primary">{user.name}</div>
							<div>{isApplicant ? 'Antragsteller' : 'Antragsbearbeiter'}</div>
						</div>
						<a href="/logout" class="btn-secondary inline-flex items-center px-3 py-2" data-testid="nav-logout">
							<LogOut class="w-4 h-4 mr-2" />
							Logout
						</a>
					{:else}
						<a href="/login" class="btn-primary inline-flex items-center px-3 py-2" data-testid="nav-login">
							<LogIn class="w-4 h-4 mr-2" />
							Login
						</a>
					{/if}
				</div>
			</div>
		</div>
	</nav>

	<main class="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
		{@render children()}
	</main>
</div>
