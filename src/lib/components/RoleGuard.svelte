<script lang="ts">
	import { page } from '$app/stores';
	import type { Snippet } from 'svelte';

	interface Props {
		requiredRole: App.UserRole;
		children: Snippet;
	}

	let { requiredRole, children }: Props = $props();

	const currentRole = $derived($page.data.user?.role ?? null);
	const hasAccess = $derived(currentRole === requiredRole);
</script>

{#if hasAccess}
	{@render children()}
{:else}
	<div class="flex items-center justify-center min-h-[50vh]" data-testid="role-guard-forbidden">
		<div class="text-center space-y-2">
			<p class="text-lg font-semibold text-primary">Keine Berechtigung</p>
			<p class="text-secondary">Bitte melden Sie sich mit einem berechtigten Konto an.</p>
			<a href="/login" class="btn-primary inline-block px-4 py-2" data-testid="role-guard-login-link">
				Zum Login
			</a>
		</div>
	</div>
{/if}
