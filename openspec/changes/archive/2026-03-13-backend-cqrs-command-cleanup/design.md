## Context

The DDD refactoring introduced a CQRS command/query pattern. However, three handlers still branch behavior based on string properties in DTOs (`Action`, `Decision`), violating the "one command = one intention" principle. Additionally, `MarkAsDeleted()` on the aggregate is a bare guard with no side effect, which is misleading.

Current state:
- `CreateApplicationHandler` / `UpdateApplicationHandler` check `dto.Action == "submit"` to optionally submit after save
- `ProcessDecisionHandler` checks `dto.Decision == "approved"` to decide between `Approve()` / `Reject()`
- `DeleteApplicationHandler` calls `MarkAsDeleted()` (pure guard) then `RemoveAsync()` (actual deletion)
- Frontend sends `POST /api/processor/{id}/decide` with `{ decision, comment }` for both approve/reject
- Frontend sends `POST /api/applications` with `{ ...data, action: "submit" }` for create-and-submit

## Goals / Non-Goals

**Goals:**
- Each command handler has exactly one domain intention — no string-based branching
- `Application.Delete()` is a proper domain operation with `ApplicationDeletedEvent`
- Validators are simpler: each validates exactly one command shape (no conditional rules like "comment required only if rejected")
- Existing frontend keeps working — endpoint changes must be minimal or backward-compatible

**Non-Goals:**
- Soft-delete implementation (pragmatic hard-delete stays)
- Domain event dispatching infrastructure (events are recorded, not yet consumed)
- Frontend refactoring (frontend adapts to new endpoints, not the other way around)

## Decisions

### 1. Delete: `MarkAsDeleted()` → `Delete()` with event

Rename `MarkAsDeleted()` to `Delete()`. The method keeps the guard (only Draft) and adds `ApplicationDeletedEvent`. The handler continues to call `RemoveAsync()` for physical deletion.

**Rationale:** The aggregate authorizes and documents the deletion (via event). The repository executes it. This is pragmatic — no soft-delete, but the domain operation is now explicit and traceable.

**Alternative considered:** Soft-delete via `Status = Deleted`. Rejected for now — adds query filter complexity without current business need. Can be introduced later by changing `Delete()` internals without touching the handler.

### 2. ProcessDecision: split into Approve + Reject

Replace `ProcessDecisionCommand` with:
- `ApproveApplicationCommand(int ApplicationId, string? Comment)`
- `RejectApplicationCommand(int ApplicationId, string Comment)` — comment required

Replace `ProcessorDecisionDto` with:
- `ApproveApplicationDto { Comment? }` 
- `RejectApplicationDto { Comment }` — comment required at type level

Replace `ProcessorDecisionValidator` with:
- `ApproveApplicationValidator` — comment optional, no decision field
- `RejectApplicationValidator` — comment required, simple `NotEmpty` rule

**Controller endpoints:** Split `POST /processor/{id}/decide` into:
- `POST /processor/{id}/approve` → `ApproveApplicationHandler`
- `POST /processor/{id}/reject` → `RejectApplicationHandler`

**Rationale:** The frontend already knows the decision (radio button selection). Sending to a decision-specific endpoint is trivial. Two endpoints are cleaner than one polymorphic endpoint. Validation is simpler per-handler.

**Frontend impact:** Change `fetch(\`/api/processor/${id}/decide\`, { body: { decision, comment } })` to `fetch(\`/api/processor/${id}/${selectedDecision}\`, { body: { comment } })`. Minimal change — `selectedDecision` already holds `"approved"` or `"rejected"`.

### 3. Create/Update: remove Action property, use query parameter

Remove `Action` property from `ApplicationCreateDto` and `ApplicationUpdateDto`.

The controller determines intent based on a query parameter:
- `POST /api/applications` → `CreateApplicationCommand` (save draft)
- `POST /api/applications?submit=true` → `CreateAndSubmitApplicationCommand` (save + submit)
- `PUT /api/applications/{id}` → `UpdateApplicationCommand` (save draft)
- `PUT /api/applications/{id}?submit=true` → `UpdateAndSubmitApplicationCommand` (update + submit)

Each "AndSubmit" handler composes the base operation with a `Submit()` call in a single transaction.

**Rationale:** Query parameter keeps the HTTP resource the same (the application), while the query parameter signals intent. The DTO stays pure data — no control flow properties. The controller is the natural place to interpret HTTP-level intent and route to the correct handler.

**Alternative considered:** Two completely separate endpoints (`POST /applications` and `POST /applications/submit`). Rejected because the body is identical — only the intent differs. Query parameter is more RESTful for this case.

**Frontend impact:** Change `submitForm('submit')` to append `?submit=true` to the fetch URL instead of adding `action: "submit"` to the body. Minimal change.

### 4. Shared validation: extract base validator

`ApplicationValidator` and `ApplicationUpdateValidator` have identical rules. Extract a base `ApplicationDataValidator` and inherit from it in both. The "AndSubmit" handlers reuse the same validator since the data shape is identical.

## Risks / Trade-offs

- **More handler classes** — 3 command files become 6. Mitigated by each being simpler and more focused.
- **More DI registrations** — 3 additional handler registrations. Trivial impact.
- **Frontend must adapt** — Decision endpoint URL changes, action parameter becomes query parameter. Both are minimal changes.
- **`ToCamelCase` duplication** — This helper exists in multiple handlers. Should be extracted to a shared utility as part of this change.
