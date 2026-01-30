# Processor Applications CSV Export Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** Add a CSV download for the processor applications list that exports all filtered results (ignoring pagination) with German headers/values.

**Architecture:** Add a dedicated export query in the applications repository, a pure CSV serialization service (unit-tested), and a SvelteKit `+server.ts` download endpoint that validates input with Zod and enforces processor role.

**Tech Stack:** SvelteKit routes (`+server.ts`), Drizzle ORM (SQLite), Zod v4, Vitest.

---

## Preconditions / Notes
- This repo already has the processor list at `src/routes/processor/+page.svelte` and server load at `src/routes/processor/+page.server.ts`.
- Current filter is `status` via query param `?status=...`.
- Current sorting is `createdAt desc`.
- CSV requirements:
  - Separator `;`
  - UTF-8 with BOM
  - German column headers
  - German values for enums (use existing label maps from `$lib/types` for consistency)
  - Date formatting: `de-DE` human readable
  - Include all fields from `applications` table
- Keep transport vs. logic separation:
  - API route: validation + auth + orchestration only
  - Repository: DB queries only
  - Service: CSV building/formatting only

## Commands
- Unit tests: `npm run test`
- Typecheck: `npm run check`
- E2E tests: `npm run test:e2e`

---

### Task 1: Add unit tests for CSV serialization (TDD)

**Files:**
- Create: `src/lib/server/services/exports/applicationCsvExport.test.ts`

**Step 1: Write the failing test**

```ts
import { describe, expect, it } from 'vitest';
import type { Application } from '$lib/server/db/schema';
import { buildApplicationsCsv } from './applicationCsvExport';

describe('buildApplicationsCsv', () => {
	it('adds UTF-8 BOM and German header row', () => {
		const csv = buildApplicationsCsv([]);
		expect(csv.startsWith('\uFEFF')).toBe(true);
		expect(csv).toContain('ID;Name;Einkommen');
	});

	it('escapes separators, quotes, and line breaks', () => {
		const apps: Application[] = [
			{
				id: 1,
				name: 'A;"B"\nC',
				income: 1000,
				fixedCosts: 100,
				desiredRate: 50,
				employmentStatus: 'employed',
				hasPaymentDefault: false,
				status: 'submitted',
				score: 80,
				trafficLight: 'green',
				scoringReasons: null,
				processorComment: null,
				createdAt: '2026-01-30T10:00:00.000Z',
				submittedAt: null,
				processedAt: null,
				createdBy: 'u1'
			}
		];

		const csv = buildApplicationsCsv(apps);
		expect(csv).toContain('"A;""B""\nC"');
	});

	it('uses German labels for enums', () => {
		const apps: Application[] = [
			{
				id: 1,
				name: 'Test',
				income: 1000,
				fixedCosts: 100,
				desiredRate: 50,
				employmentStatus: 'self_employed',
				hasPaymentDefault: true,
				status: 'approved',
				score: null,
				trafficLight: 'yellow',
				scoringReasons: null,
				processorComment: null,
				createdAt: '2026-01-30T10:00:00.000Z',
				submittedAt: null,
				processedAt: null,
				createdBy: 'u1'
			}
		];

		const csv = buildApplicationsCsv(apps);
		expect(csv).toContain('Selbstständig');
		expect(csv).toContain('Genehmigt');
		// trafficLightLabels currently map to German risk labels
		expect(csv).toContain('Prüfung erforderlich');
	});
});
```

**Step 2: Run test to verify it fails**

Run: `npm run test`
Expected: FAIL with module not found `./applicationCsvExport` or missing export.

**Step 3: Commit (optional)**

Skip if you do not want commits.

---

### Task 2: Implement CSV serialization service (minimal to make tests pass)

**Files:**
- Create: `src/lib/server/services/exports/applicationCsvExport.ts`

**Step 1: Implement minimal `buildApplicationsCsv`**

```ts
import type { Application } from '$lib/server/db/schema';
import { employmentStatusLabels, statusLabels, trafficLightLabels } from '$lib/types';

function escapeCsvCell(value: string): string {
	if (value.includes(';') || value.includes('"') || value.includes('\n') || value.includes('\r')) {
		return `"${value.replaceAll('"', '""')}"`;
	}
	return value;
}

function formatDateTimeDe(value: string | null): string {
	if (!value) return '';
	const date = new Date(value);
	if (Number.isNaN(date.getTime())) return '';
	return date.toLocaleString('de-DE', {
		year: 'numeric',
		month: '2-digit',
		day: '2-digit',
		hour: '2-digit',
		minute: '2-digit'
	});
}

function boolToDe(value: boolean): string {
	return value ? 'Ja' : 'Nein';
}

export function buildApplicationsCsv(apps: Application[]): string {
	const header = [
		'ID',
		'Name',
		'Einkommen',
		'Fixkosten',
		'Gewünschte Rate',
		'Beschäftigungsstatus',
		'Zahlungsausfall',
		'Status',
		'Score',
		'Ampel',
		'Scoring Gründe',
		'Kommentar (Sachbearbeitung)',
		'Erstellt am',
		'Eingereicht am',
		'Bearbeitet am',
		'Erstellt von'
	];

	const rows = apps.map((a) => {
		const score = a.score === null ? '' : String(a.score);
		const trafficLight = a.trafficLight ? (trafficLightLabels[a.trafficLight] ?? a.trafficLight) : '';
		const employment = employmentStatusLabels[a.employmentStatus] ?? a.employmentStatus;
		const status = statusLabels[a.status] ?? a.status;

		return [
			String(a.id),
			a.name,
			String(a.income),
			String(a.fixedCosts),
			String(a.desiredRate),
			employment,
			boolToDe(a.hasPaymentDefault),
			status,
			score,
			trafficLight,
			a.scoringReasons ?? '',
			a.processorComment ?? '',
			formatDateTimeDe(a.createdAt),
			formatDateTimeDe(a.submittedAt),
			formatDateTimeDe(a.processedAt),
			a.createdBy
		].map(escapeCsvCell).join(';');
	});

	// UTF-8 BOM to make Excel happy
	return `\uFEFF${[header.join(';'), ...rows].join('\n')}\n`;
}
```

**Step 2: Run tests**

Run: `npm run test`
Expected: PASS for the new suite.

**Step 3: Commit (optional)**

Skip if you do not want commits.

---

### Task 3: Add repository function to fetch export data (no pagination)

**Files:**
- Modify: `src/lib/server/services/repositories/application.repository.ts`

**Step 1: Write a small unit test (optional)**

Skip unless repository functions are already tested elsewhere.

**Step 2: Add a new function**

Add a function that mirrors the list filter/sort but without pagination:

```ts
export async function getProcessorApplicationsForExport(params: {
	status?: ApplicationStatus;
}): Promise<Application[]> {
	const whereClause = params.status ? eq(applications.status, params.status) : undefined;
	const query = db
		.select()
		.from(applications)
		.orderBy(desc(applications.createdAt));

	return whereClause ? query.where(whereClause).all() : query.all();
}
```

**Step 3: Run unit tests**

Run: `npm run test`
Expected: PASS.

**Step 4: Commit (optional)**

Skip if you do not want commits.

---

### Task 4: Create CSV download API route (transport layer)

**Files:**
- Create: `src/routes/api/processor/applications.csv/+server.ts`

**Step 1: Add the endpoint with validation and auth checks**

```ts
import type { RequestHandler } from './$types';
import { error } from '@sveltejs/kit';
import { z } from 'zod';
import type { ApplicationStatus } from '$lib/server/db/schema';
import { getProcessorApplicationsForExport } from '$lib/server/services/repositories/application.repository';
import { buildApplicationsCsv } from '$lib/server/services/exports/applicationCsvExport';

const querySchema = z.object({
	status: z.enum(['draft', 'submitted', 'approved', 'rejected'] as const).optional()
});

function normalizeQuery(url: URL): { status?: ApplicationStatus } {
	const raw = Object.fromEntries(url.searchParams);
	// Treat empty string as undefined
	if (raw.status === '') {
		delete (raw as Record<string, string>).status;
	}
	return querySchema.parse(raw);
}

export const GET: RequestHandler = async ({ url, locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}
	if (locals.user.role !== 'processor') {
		throw error(403, 'Keine Berechtigung');
	}

	const query = normalizeQuery(url);
	const apps = await getProcessorApplicationsForExport({ status: query.status });
	const csv = buildApplicationsCsv(apps);

	const today = new Date().toISOString().slice(0, 10);
	return new Response(csv, {
		headers: {
			'content-type': 'text/csv; charset=utf-8',
			'content-disposition': `attachment; filename="antraege-processor-${today}.csv"`
		}
	});
};
```

**Step 2: Manual check (dev server)**

Run: `npm run dev`
Expected: Visiting `/api/processor/applications.csv` while logged in as processor triggers a download.

**Step 3: Commit (optional)**

Skip if you do not want commits.

---

### Task 5: Add export button to processor list UI

**Files:**
- Modify: `src/routes/processor/+page.svelte`

**Step 1: Add a link/button that preserves current filters**

Add a computed export URL based on `$page.url.search`:

```svelte
<script lang="ts">
	// ... existing imports
	const exportHref = $derived(() => {
		const url = new URL($page.url);
		url.pathname = '/api/processor/applications.csv';
		url.searchParams.delete('page');
		return url.toString();
	});
</script>
```

Add the button near the filter row:

```svelte
<a
	href={exportHref}
	class="btn btn-secondary"
	data-testid="processor-applications-export-csv-button"
>
	CSV herunterladen
</a>
```

If the project does not have `btn` classes, reuse existing button styling patterns from other pages/components.

**Step 2: Manual check**
- Apply a status filter
- Click export
Expected: Download includes only matching rows.

**Step 3: Commit (optional)**

Skip if you do not want commits.

---

### Task 6: Verification pass

**Files:**
- None

**Step 1: Run unit tests**

Run: `npm run test`
Expected: PASS

**Step 2: Run typecheck**

Run: `npm run check`
Expected: PASS

**Step 3: Run E2E tests (optional)**

Run: `npm run test:e2e`
Expected: PASS

---

## Acceptance Criteria Checklist
- Processor list page has an export trigger with stable `data-testid="processor-applications-export-csv-button"`.
- Export endpoint is `GET /api/processor/applications.csv`.
- Export respects current `status` filter and uses `createdAt desc` sorting.
- Export ignores pagination and returns all matching records.
- CSV is `;` separated, UTF-8 with BOM, German headers, German enum values, and de-DE date format.
- Unauthorized access returns 401/403 without redirects.
