<script lang="ts">
	import { goto } from '$app/navigation';
	import { browser } from '$app/environment';
	import { currentUser, type UserRole } from '$lib/stores/role';
	import type { Snippet } from 'svelte';

	interface Props {
		requiredRole: UserRole;
		redirectTo: string;
		children: Snippet;
	}

	let { requiredRole, redirectTo, children }: Props = $props();

	$effect(() => {
		if (browser && $currentUser.role !== requiredRole) {
			goto(redirectTo);
		}
	});

	const hasAccess = $derived($currentUser.role === requiredRole);
</script>

{#if hasAccess}
	{@render children()}
{:else}
	<div class="flex items-center justify-center min-h-[50vh]">
		<div class="text-center">
			<p class="text-secondary">Zugriff nicht erlaubt. Sie werden weitergeleitet...</p>
		</div>
	</div>
{/if}
