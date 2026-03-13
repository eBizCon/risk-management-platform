## Context

The backend API currently embeds frontend route paths (e.g. `/applications/{id}`) in command result records and controller responses via a `Redirect` string property. This pattern exists in 6 command result records and is consumed by 2 controllers (`ApplicationsController`, `ProcessorController`). On the frontend side, only `ApplicationForm.svelte` uses `result.redirect` — the processor detail page already handles navigation client-side.

The `Redirect` property was introduced during the initial CQRS command split but is architecturally misplaced: the Application Layer should not know about frontend routes.

## Goals / Non-Goals

**Goals:**
- Remove `Redirect` from all command result records in the Application Layer
- Unify controller response patterns — all endpoints use `result.ToActionResult()`
- Move navigation responsibility entirely to the frontend
- Update the `cqrs-command-split` spec to reflect the new contract

**Non-Goals:**
- Changing the `ToActionResult()` extension method itself
- Modifying query responses or the `ApplicationResponse` DTO
- Changing how the processor detail page handles navigation (already correct)
- Introducing a generic "next action" or HATEOAS pattern

## Decisions

### Decision 1: Remove Redirect entirely (no replacement)

**Choice:** Remove the `Redirect` property from result records. Do not replace it with any alternative server-side navigation hint.

**Rationale:** The frontend already receives `application.id` (and `application.status`) in every response. This is sufficient to construct any navigation target. A HATEOAS-style `links` object would be over-engineering for a single SPA consumer.

**Alternatives considered:**
- **HATEOAS links**: Too complex for the current architecture with a single frontend consumer
- **Keep redirect but move to controller layer only**: Still couples controller to frontend routes

### Decision 2: Use `result.ToActionResult()` consistently

**Choice:** All command endpoints use the same `result.ToActionResult()` pattern already used by query endpoints and `SubmitApplication`.

**Rationale:** Eliminates the special-case anonymous objects (`new { application, redirect }`) and the manual `IsSuccess` checks in controllers. The `ToActionResult()` extension already handles success/error mapping correctly.

### Decision 3: Frontend constructs navigation from application.id

**Choice:** `ApplicationForm.svelte` navigates to `/applications/{id}` after success using the application object from the response.

**Rationale:** The form already has the `application` field in the response. The navigation target is a simple string interpolation — no lookup or additional API call needed.

## Risks / Trade-offs

- **Risk: Spec scenarios reference redirect** → Mitigation: Delta spec removes redirect from all affected scenarios
- **Risk: Future consumers might need navigation hints** → Mitigation: Can be re-introduced as HATEOAS links if needed; current removal is easily reversible
- **Trade-off: Frontend now "knows" its own routes** → This is correct by design; the frontend should own its routing
