<script lang="ts">
	import { currentUser, debugMode } from '$lib/stores/role';
	import { User, UserCog } from 'lucide-svelte';
</script>

{#if $debugMode}
	<div class="fixed bottom-4 right-4 bg-white rounded-lg shadow-lg border border-gray-200 p-4 z-50">
		<div class="text-xs text-gray-500 mb-2">Debug: Rollenwechsel</div>
		<div class="flex items-center gap-2 mb-3">
			<div class="w-8 h-8 rounded-full bg-gray-100 flex items-center justify-center">
				{#if $currentUser.role === 'applicant'}
					<User class="w-4 h-4 text-gray-600" />
				{:else}
					<UserCog class="w-4 h-4 text-gray-600" />
				{/if}
			</div>
			<div>
				<div class="text-sm font-medium">{$currentUser.name}</div>
				<div class="text-xs text-gray-500">
					{$currentUser.role === 'applicant' ? 'Antragsteller' : 'Antragsbearbeiter'}
				</div>
			</div>
		</div>
		<button
			onclick={() => currentUser.switchRole()}
			class="w-full px-3 py-2 text-sm bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors"
		>
			Rolle wechseln
		</button>
	</div>
{/if}
