<script lang="ts">
	interface Customer {
		id: number;
		name: string;
		balance: number;
		userLogin: string | null;
	}

	interface Props {
		selectedCustomerId?: number | null;
		errors?: Record<string, string[]>;
	}

	let { selectedCustomerId = null, errors = {} }: Props = $props();

	let customers = $state<Customer[]>([]);
	let loading = $state(true);
	let fetchError = $state(false);

	$effect(() => {
		loadCustomers();
	});

	async function loadCustomers() {
		try {
			loading = true;
			fetchError = false;
			const response = await fetch('/api/customers');
			if (response.ok) {
				customers = await response.json();
			} else {
				fetchError = true;
			}
		} catch {
			fetchError = true;
		} finally {
			loading = false;
		}
	}

	function formatCurrency(value: number): string {
		return new Intl.NumberFormat('de-DE', {
			style: 'currency',
			currency: 'EUR'
		}).format(value);
	}
</script>

<div>
	<label for="customerId" class="form-label block">Kunde (optional)</label>
	{#if loading}
		<p class="mt-1 text-sm text-secondary">Kundendaten werden geladen...</p>
		<input type="hidden" name="customerId" value="" />
	{:else if fetchError}
		<p class="mt-1 text-sm text-secondary">
			Kundendaten konnten nicht geladen werden. Die JHipster-App ist möglicherweise nicht erreichbar.
		</p>
		<input type="hidden" name="customerId" value="" />
	{:else}
		<select
			id="customerId"
			name="customerId"
			class="mt-1 block w-full rounded-md border-default shadow-sm sm:text-sm"
			data-testid="select-customer"
		>
			<option value="">Kein Kunde zugeordnet</option>
			{#each customers as customer}
				<option value={customer.id} selected={selectedCustomerId === customer.id}>
					{customer.name} — {formatCurrency(customer.balance)}
					{#if customer.userLogin}
						({customer.userLogin})
					{/if}
				</option>
			{/each}
		</select>
	{/if}
	{#if errors.customerId}
		<p class="mt-1 error-text">{errors.customerId[0]}</p>
	{/if}
</div>
