## Why

Users currently land on a generic homepage without insight into their application status. Applicants and processors need immediate visibility into their workload - applicants want to see their own applications, processors need an overview of all pending work.

## What Changes

- Add dashboard section to homepage (`/`) with role-based statistics
- Display stats cards showing counts per status (Draft, Eingereicht, Genehmigt, Abgelehnt)
- Add bar chart showing "Antrag nach Status" distribution
- Add pie chart showing percentage distribution ("Verteilung")
- Install `svelte-chartjs` for chart rendering
- Add repository function to aggregate dashboard statistics

## Capabilities

### New Capabilities

- `application-dashboard`: Dashboard component with stats cards and charts, role-based data filtering (applicant sees own applications, processor sees all), status grouping (submitted + needs_information + resubmitted = "Eingereicht")

### Modified Capabilities

(none - this is a new feature, no existing requirements change)

## Impact

- **Frontend**: `src/routes/+page.svelte`, `src/routes/+page.server.ts`
- **Backend**: `src/lib/server/services/repositories/application.repository.ts`
- **Dependencies**: Add `svelte-chartjs` and `chart.js`
- **Database**: No schema changes
