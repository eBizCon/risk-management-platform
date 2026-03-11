## Context

The current application domain already supports applicant and processor roles, role-based route protection, application creation and submission, processor review, and final decisions. Applications currently move through `draft`, `submitted`, `approved`, and `rejected`, with scoring recalculated during create, update, and submit operations. The change introduces a clarification loop into an existing workflow and therefore affects domain data, workflow rules, validation, authorization, detail views, and tests.

The project enforces repository-based data access, service-layer business logic, and Zod validation for incoming data. API routes must return `401` and `403` responses instead of redirecting, and frontend behavior must show an appropriate unauthorized state instead of forcing navigation.

## Goals / Non-Goals

**Goals:**
- Add a structured inquiry workflow between processor and applicant.
- Extend the application workflow with `needs_information` and `resubmitted` states.
- Persist inquiry records and responses as part of the application history.
- Enforce role- and status-based rules for creating and responding to inquiries.
- Surface inquiries in applicant and processor application detail views.
- Keep the first version small enough to implement without introducing messaging infrastructure.

**Non-Goals:**
- Real-time chat or multi-threaded conversations.
- Email or push notifications.
- File attachments as part of inquiry responses.
- Multiple simultaneously open inquiry threads for one application.

## Decisions

### Store inquiries as a dedicated persistence model linked to applications
A dedicated inquiry entity is preferable to overloading the application table with ad hoc question and answer fields. This keeps the workflow extensible, preserves history, and aligns with the repository pattern by allowing an inquiry repository dedicated to this entity.

Alternative considered:
- Store the latest inquiry fields directly on the application record. Rejected because it does not support historical tracking cleanly and becomes brittle when more than one inquiry occurs over time.

### Treat inquiry handling as workflow logic coordinated through services
Creating an inquiry and answering an inquiry both change application state and create or update inquiry records. This is cross-entity workflow logic and should therefore live in a dedicated service rather than in routes or raw repositories.

Alternative considered:
- Put all workflow rules into repositories. Rejected because the change spans multiple entities and requires authorization and transition logic that is better expressed and tested in services.

### Allow only one open inquiry at a time in the first version
The first implementation should keep inquiry flow sequential. A processor may ask multiple inquiries over an application's lifetime, but only after the previous one has been answered. This keeps status semantics simple and avoids parallel response state.

Alternative considered:
- Allow multiple concurrent open inquiries. Rejected for the first version because it complicates both UI and state transitions.

### Expose inquiry actions through page actions or API endpoints with Zod validation
Incoming processor inquiry creation and applicant response payloads must be validated with Zod before invoking services. The existing application detail pages already provide a natural place for these actions, while API handlers remain useful for explicit machine-readable access and testability.

Alternative considered:
- Use only client-side validation. Rejected because server-side validation is required by project rules and is necessary for authorization-safe workflow changes.

## Risks / Trade-offs

- [Workflow complexity increases] → Mitigation: keep the first version limited to one open inquiry and two new statuses.
- [Status handling may become inconsistent across UI and server logic] → Mitigation: centralize transition rules in a workflow service and update shared status label/type definitions.
- [Authorization bugs may expose inquiry content to the wrong applicant] → Mitigation: enforce ownership checks in services and API/page actions, with explicit tests for `401` and `403` cases.
- [Schema changes affect existing application queries and tests] → Mitigation: keep the application schema extension small and introduce a dedicated inquiry table instead of broad application table denormalization.

## Migration Plan

1. Add schema changes for inquiry persistence and application status enum updates.
2. Add repository support for inquiry creation, retrieval, and response updates.
3. Add service-layer workflow operations for processor inquiry creation and applicant response handling.
4. Extend detail pages and actions for processor and applicant flows.
5. Add or update tests for status transitions, authorization, and rendering of inquiry history.
6. Seed data may optionally be extended later but is not required for the first delivery.

Rollback strategy:
- Remove inquiry UI and service usage first.
- Revert schema additions and status changes if the feature is not yet in use.
- If data already exists, archive inquiry records before rolling back persistence changes.

## Open Questions

- Should a processor be able to set an optional response deadline in the first implementation, or should this remain a later extension?
- Should `resubmitted` appear as a dedicated list filter everywhere, or only in processor-facing views initially?
- Should processor comments used for approve/reject remain separate from inquiries, or should all review communication later be unified into one timeline?
