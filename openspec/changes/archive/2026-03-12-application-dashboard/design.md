## Context

The homepage currently shows a generic welcome message with feature cards and role-based CTAs. Users need immediate visibility into their application status upon login. This design adds a dashboard section with statistics and charts.

**Current State:**
- Homepage (`/`) has hero section, feature cards, and CTAs
- Processor page (`/processor`) has stats cards but no charts
- No chart library installed

**Constraints:**
- Follow existing Repository Pattern (backend-architecture.md)
- Use TailwindCSS 4.0 styling (styling-css.md)
- Svelte 5 runes ($state, $derived, $props)
- Responsive design (360px minimum)
- Role-based data: applicant sees own applications, processor sees all

## Goals / Non-Goals

**Goals:**
- Add dashboard section to homepage with stats cards and charts
- Implement role-based data filtering
- Group status values for display (submitted + needs_information + resubmitted = "Eingereicht")
- Install and configure svelte-chartjs

**Non-Goals:**
- Replace or modify the `/processor` page dashboard
- Add filtering or drill-down functionality
- Real-time updates (data loads on page load)

## Decisions

### D1: Chart Library - svelte-chartjs

**Decision:** Use `svelte-chartjs` (Chart.js wrapper for Svelte)

**Rationale:**
- Lightweight and well-maintained
- Chart.js has excellent documentation
- Svelte-native integration
- Supports bar and pie charts out of the box

**Alternatives Considered:**
- LayerChart: More Svelte-native but heavier, more dependencies
- Apache ECharts: Feature-rich but overkill for two simple charts
- D3 directly: Too much boilerplate for this use case

### D2: Data Loading - Server-Side

**Decision:** Load dashboard data in `+page.server.ts`

**Rationale:**
- Follows existing pattern (processor page loads stats server-side)
- No additional API endpoint needed
- Data available immediately on page load
- Consistent with SvelteKit best practices

### D3: Status Grouping

**Decision:** Group statuses for display:
- Draft → "Entwurf"
- Submitted + needs_information + resubmitted → "Eingereicht"
- Approved → "Genehmigt"
- Rejected → "Abgelehnt"

**Rationale:**
- Matches Figma design (4 status categories)
- "Eingereicht" represents all in-flight applications needing attention
- Simpler visualization for users

### D4: Component Structure

**Decision:** Create reusable chart components:
- `DashboardCharts.svelte` - Container with both charts
- `StatusBarChart.svelte` - Bar chart component
- `StatusPieChart.svelte` - Pie chart component

**Rationale:**
- Separation of concerns
- Reusable if needed elsewhere
- Easier testing

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Chart.js bundle size (~60KB) | Acceptable for dashboard feature; lazy load if needed |
| No data for new users | Show empty state with "0" values and CTA to create application |
| Chart accessibility | Chart.js has limited ARIA support; add textual summary above charts |
| Mobile chart readability | Charts are responsive; stack vertically on mobile |
