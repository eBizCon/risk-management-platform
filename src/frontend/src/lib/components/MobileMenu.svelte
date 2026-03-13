<script lang="ts">
	import { Home, FileText, ClipboardList, LogOut, X } from 'lucide-svelte';
	import type { User } from '$lib/types';

	const { isOpen, onClose, user, isApplicant, isProcessor } = $props<{
		isOpen: boolean;
		onClose: () => void;
		user: User | null;
		isApplicant: boolean;
		isProcessor: boolean;
	}>();

	$effect(() => {
		if (typeof document === 'undefined') return;
		if (!isOpen) return;

		const previousOverflow = document.body.style.overflow;
		document.body.style.overflow = 'hidden';

		const handleKeyDown = (event: KeyboardEvent) => {
			if (event.key === 'Escape') {
				onClose?.();
			}
		};

		document.addEventListener('keydown', handleKeyDown);

		return () => {
			document.removeEventListener('keydown', handleKeyDown);
			document.body.style.overflow = previousOverflow;
		};
	});
</script>

{#if isOpen}
	<button
		type="button"
		class="fixed inset-0 z-40"
		aria-label="Schließe Navigation"
		data-testid="mobile-menu-overlay"
		onclick={onClose}
	>
		<div class="absolute inset-0 bg-black/50"></div>
	</button>
{/if}

<div
	class={`fixed inset-y-0 left-0 z-50 w-72 max-w-full bg-white shadow-xl transform transition-transform duration-300 ${isOpen ? 'translate-x-0' : '-translate-x-full'}`}
	data-testid="mobile-menu"
>
	<div class="flex flex-col h-full">
		<div class="flex items-center justify-between px-4 py-4 border-b border-border">
			<a href="/" class="text-lg font-semibold text-brand">Risikomanagement</a>
			<button
				type="button"
				class="btn-secondary inline-flex items-center justify-center w-10 h-10"
				onclick={onClose}
				data-testid="mobile-menu-close"
			>
				<X class="w-5 h-5" />
			</button>
		</div>

		<nav class="flex-1 overflow-y-auto px-4 py-4 space-y-2">
			<a href="/" class="nav-link flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium">
				<Home class="w-4 h-4" />
				<span>Startseite</span>
			</a>
			{#if isApplicant}
				<a
					href="/applications"
					class="nav-link flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium"
				>
					<FileText class="w-4 h-4" />
					<span>Meine Anträge</span>
				</a>
				<a
					href="/applications/new"
					class="nav-link flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium"
				>
					<ClipboardList class="w-4 h-4" />
					<span>Neuer Antrag</span>
				</a>
			{/if}
			{#if isProcessor}
				<a
					href="/processor"
					class="nav-link flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium"
				>
					<ClipboardList class="w-4 h-4" />
					<span>Anträge bearbeiten</span>
				</a>
			{/if}
		</nav>

		<div class="border-t border-border px-4 py-4 space-y-2">
			{#if user}
				<div class="text-sm">
					<div class="font-medium text-primary">{user.name}</div>
					<div class="text-secondary">{isApplicant ? 'Antragsteller' : 'Antragsbearbeiter'}</div>
				</div>
				<a
					href="/logout"
					class="btn-secondary w-full inline-flex items-center justify-center gap-2"
					data-testid="mobile-menu-logout"
				>
					<LogOut class="w-4 h-4" />
					<span>Logout</span>
				</a>
			{:else}
				<a
					href="/login"
					class="btn-primary w-full inline-flex items-center justify-center gap-2"
					data-testid="mobile-menu-login"
				>
					<LogOut class="w-4 h-4" />
					<span>Login</span>
				</a>
			{/if}
		</div>
	</div>
</div>
