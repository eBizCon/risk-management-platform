<script lang="ts">
	let { page, totalPages, onPageChange, testIdPrefix = 'processor' }: { page: number; totalPages: number; onPageChange: (page: number) => void; testIdPrefix?: string } = $props();

	const visiblePageCount = 5;

	function getVisiblePages(current: number, total: number): number[] {
		if (total <= visiblePageCount) {
			return Array.from({ length: total }, (_, index) => index + 1);
		}

		if (current <= 3) {
			return [1, 2, 3, 4, 5];
		}

		if (current >= total - 2) {
			return [total - 4, total - 3, total - 2, total - 1, total];
		}

		return [current - 2, current - 1, current, current + 1, current + 2];
	}

	function handleSelect(targetPage: number) {
		if (targetPage < 1 || targetPage > totalPages || targetPage === page) {
			return;
		}
		onPageChange(targetPage);
	}

	const visiblePages = $derived(getVisiblePages(page, totalPages));
	const showLeadingEllipsis = $derived(visiblePages[0] > 1);
	const showTrailingEllipsis = $derived(visiblePages[visiblePages.length - 1] < totalPages);
	const isFirst = $derived(page === 1);
	const isLast = $derived(page === totalPages);
</script>

<nav class="flex items-center justify-center gap-3" aria-label="Pagination" data-testid="{testIdPrefix}-pagination">
	<button
		type="button"
		class="btn ghost btn-sm px-3 rounded-md"
		onclick={() => handleSelect(1)}
		disabled={isFirst}
		data-testid="{testIdPrefix}-pagination-first"
	>
		«
	</button>
	<button
		type="button"
		class="btn ghost btn-sm px-3 rounded-md"
		onclick={() => handleSelect(page - 1)}
		disabled={isFirst}
		data-testid="{testIdPrefix}-pagination-prev"
	>
		‹
	</button>

	{#if showLeadingEllipsis}
		<button
			type="button"
			class="btn ghost btn-sm px-3 rounded-md"
			onclick={() => handleSelect(1)}
			data-testid="{testIdPrefix}-pagination-page-1"
		>
			1
		</button>
		<span class="text-secondary px-1" data-testid="{testIdPrefix}-pagination-ellipsis-start">…</span>
	{/if}

	{#each visiblePages as visiblePage}
		<button
			type="button"
			class={`btn btn-sm px-3 rounded-md ${visiblePage === page ? 'btn-primary' : 'ghost'}`}
			onclick={() => handleSelect(visiblePage)}
			aria-current={visiblePage === page ? 'page' : undefined}
			data-testid={`${testIdPrefix}-pagination-page-${visiblePage}`}
		>
			{visiblePage}
		</button>
	{/each}

	{#if showTrailingEllipsis}
		<span class="text-secondary px-1" data-testid="{testIdPrefix}-pagination-ellipsis-end">…</span>
		<button
			type="button"
			class="btn ghost btn-sm px-3 rounded-md"
			onclick={() => handleSelect(totalPages)}
			data-testid={`${testIdPrefix}-pagination-page-${totalPages}`}
		>
			{totalPages}
		</button>
	{/if}

	<button
		type="button"
		class="btn ghost btn-sm px-3 rounded-md"
		onclick={() => handleSelect(page + 1)}
		disabled={isLast}
		data-testid="{testIdPrefix}-pagination-next"
	>
		›
	</button>
	<button
		type="button"
		class="btn ghost btn-sm px-3 rounded-md"
		onclick={() => handleSelect(totalPages)}
		disabled={isLast}
		data-testid="{testIdPrefix}-pagination-last"
	>
		»
	</button>
</nav>
