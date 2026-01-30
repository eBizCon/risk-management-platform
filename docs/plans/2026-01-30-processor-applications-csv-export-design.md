---
topic: Processor Applications CSV Export
created: 2026-01-30
status: draft
---

# Goal
Provide processors with a CSV download for the "Meine Anträge" (processor applications) list. The export must respect the currently active filter(s) and sorting, and include all fields available in the applications data model.

# Scope
- Add a "CSV herunterladen" button to the processor applications list page.
- Export all matching applications (ignore pagination).
- Include all fields from the database model `applications`.

# Non-goals
- Adding new filter types beyond what the list already supports.
- Adding a new sorting UI.

# Current State
- Page: `src/routes/processor/+page.svelte`
- Data load: `src/routes/processor/+page.server.ts`
  - Filter: `status` via query param `?status=...`
  - Sorting: server-side fixed `createdAt desc`
  - Pagination: `page` / `PAGE_SIZE`
- Data model: `src/lib/server/db/schema.ts` (`applications` table)

# Requirements
## Functional
- The processor can trigger a CSV download via a button on the processor applications list page.
- The CSV contains all applications that match the current filter(s).
- The export uses the same sort order as the list: `createdAt desc`.

## CSV Format
- Separator: `;`
- Encoding: UTF-8 with BOM
- Header labels: German
- Enum values: German
  - Status (e.g. `Eingereicht`, `Genehmigt`, `Abgelehnt`, `Entwurf`)
  - Employment status (e.g. `Angestellt`, `Selbstständig`, `Arbeitslos`, `Rentner`)
  - Traffic light (e.g. `Rot`, `Gelb`, `Grün`)
- Date/time formatting: German/Excel friendly, e.g. `30.01.2026 11:55`
- Null/undefined values are serialized as empty CSV cells.
- Proper CSV escaping is required:
  - Fields containing `;`, `"`, or line breaks are quoted with `"..."`
  - Quotes inside fields are escaped as `""`

## Data Columns
Include all columns from `applications`:
- `id`
- `name`
- `income`
- `fixedCosts`
- `desiredRate`
- `employmentStatus`
- `hasPaymentDefault`
- `status`
- `score`
- `trafficLight`
- `scoringReasons`
- `processorComment`
- `createdAt`
- `submittedAt`
- `processedAt`
- `createdBy`

# Proposed Approach
## Recommended: Server-side export via API route
### Transport layer
- Add API endpoint:
  - `GET /api/processor/applications.csv`
  - Query params:
    - `status` optional, same allowed values as the list

### Validation
- Use Zod schema to validate query params before querying.

### Authorization
- Require authenticated user.
- Require role `processor`.
- On auth failure:
  - Return `401` if not logged in
  - Return `403` if role is not `processor`
- Do not redirect.

### Repository layer
- Extend `src/lib/server/services/repositories/application.repository.ts` with a dedicated export query function, for example:
  - `getProcessorApplicationsForExport({ status })`
  - No pagination
  - `orderBy desc(createdAt)`

### CSV generation
- Implement CSV serialization in the route (or a small server-side utility module used by the route).
- Add BOM at the beginning of the CSV output.

### Response
- `Content-Type: text/csv; charset=utf-8`
- `Content-Disposition: attachment; filename="antraege-processor-YYYY-MM-DD.csv"`

# UI Changes
- Add a CSV export button to `src/routes/processor/+page.svelte`.
- Add stable test id:
  - `data-testid="processor-applications-export-csv-button"`
- The button should use the current URL search params (e.g. `status`) when building the export URL.

# Error Handling
- If the export fails due to server errors, return a non-2xx response.
- The UI should not attempt an automatic redirect; errors can be surfaced via a toast or inline message later (implementation detail).

# Testing Strategy
- Unit tests (Vitest) for CSV serialization:
  - Escaping of separators, quotes, and line breaks
  - BOM presence
  - Date formatting
  - Enum mapping to German labels
- Endpoint behavior tests (optional, if existing patterns exist):
  - 401/403 behavior
  - Content headers set correctly
- E2E (Playwright):
  - Export button exists
  - Download request is triggered with the active filter

# Acceptance Criteria
- Processor can download a CSV from the processor applications list.
- CSV contains all applications matching the current status filter.
- CSV includes all fields from the applications data model.
- CSV uses `;` separator, UTF-8 with BOM, German headers, German enum values, and de-DE date format.
- API returns 401/403 on unauthorized access without redirects.
