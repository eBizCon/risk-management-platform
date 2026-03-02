<script lang="ts">
	interface Customer {
		id: number;
		name: string;
		balance: number;
		userLogin: string | null;
	}

	interface Props {
		customerId: number | null;
	}

	let { customerId }: Props = $props();

	let customer = $state<Customer | null>(null);
	let loading = $state(false);
	let fetchError = $state(false);

	$effect(() => {
		if (customerId) {
			loadCustomer(customerId);
		} else {
			customer = null;
			loading = false;
			fetchError = false;
		}
	});

	async function loadCustomer(id: number) {
		try {
			loading = true;
			fetchError = false;
			const response = await fetch(`/api/customers/${id}`);
			if (response.ok) {
				customer = await response.json();
			} else {
				fetchError = true;
				customer = null;
			}
		} catch {
			fetchError = true;
			customer = null;
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

{#if customerId}
	<div class="card p-6" data-testid="customer-info">
		<h2 class="text-lg font-semibold text-primary mb-4">Kundendaten</h2>
		{#if loading}
			<p class="text-sm text-secondary">Kundendaten werden geladen...</p>
		{:else if fetchError}
			<p class="text-sm text-secondary">
				Kundendaten konnten nicht geladen werden (Kunde #{customerId}).
			</p>
		{:else if customer}
			<dl class="grid grid-cols-1 sm:grid-cols-2 gap-4">
				<div>
					<dt class="dl-label">Kundenname</dt>
					<dd class="mt-1 dl-value" data-testid="customer-name">{customer.name}</dd>
				</div>
				<div>
					<dt class="dl-label">Kontostand</dt>
					<dd class="mt-1 dl-value" data-testid="customer-balance">{formatCurrency(customer.balance)}</dd>
				</div>
				{#if customer.userLogin}
					<div>
						<dt class="dl-label">Benutzer</dt>
						<dd class="mt-1 dl-value" data-testid="customer-user-login">{customer.userLogin}</dd>
					</div>
				{/if}
				<div>
					<dt class="dl-label">Kunden-ID</dt>
					<dd class="mt-1 dl-value" data-testid="customer-id">{customer.id}</dd>
				</div>
			</dl>
		{/if}
	</div>
{/if}
