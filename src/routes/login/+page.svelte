<script lang="ts">
	const props = $props<{ data?: { title?: string } }>();

	const title = $derived(props.data?.title ?? 'Anmeldung');
</script>

<svelte:head>
	<title>{title}</title>
</svelte:head>

<div class="min-h-screen flex items-center justify-center bg-linear-to-br text-white px-6">
	<div
		class="max-w-md w-full space-y-8 p-8 rounded-2xl bg-slate-900/90 backdrop-blur border border-slate-800 shadow-2xl"
	>
		<div class="space-y-2 text-center">
			<p class="text-sm uppercase tracking-[0.2em] text-slate-400">Risk Management</p>
			<h1 class="text-3xl font-semibold">Melde dich an</h1>
			<p class="text-sm text-slate-400">
				Starte den sicheren Login über unseren Identitätsprovider.
			</p>
		</div>

		<form method="post" action="/login" class="space-y-4">
			<button
				type="submit"
				data-testid="auth-login-button"
				class="w-full inline-flex items-center justify-center rounded-lg bg-emerald-500 px-4 py-3 text-sm font-semibold text-slate-950 shadow-lg shadow-emerald-500/30 transition hover:bg-emerald-400 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-300"
			>
				Login
			</button>
		</form>
	</div>
</div>
