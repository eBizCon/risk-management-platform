<script lang="ts">
	import { currentUser, debugMode } from '$lib/stores/role';
	import { User, UserCog } from 'lucide-svelte';
</script>

{#if $debugMode}
	<div class="fixed bottom-4 left-4 card rounded-lg shadow-lg p-4 z-50">
		<div class="text-xs text-secondary mb-2">Debug: Rollenwechsel</div>
		<div class="flex items-center gap-2 mb-3">
			<div class="w-8 h-8 rounded-full card-muted flex items-center justify-center">
				{#if $currentUser.role === 'applicant'}
					<User class="w-4 h-4 text-secondary" />
				{:else}
					<UserCog class="w-4 h-4 text-secondary" />
				{/if}
			</div>
			<div>
				<div class="text-sm font-medium text-primary">{$currentUser.name}</div>
				<div class="text-xs text-secondary">
					{$currentUser.role === 'applicant' ? 'Antragsteller' : 'Antragsbearbeiter'}
				</div>
			</div>
		</div>
		<button
			onclick={() => currentUser.switchRole()}
			class="btn-primary w-full"
		>
			Rolle wechseln
		</button>
	</div>
{/if}
