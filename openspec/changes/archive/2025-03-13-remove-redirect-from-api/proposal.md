## Why

The backend API currently returns a `redirect` property (a frontend route path) in command result records and controller responses. This violates separation of concerns: the Application Layer hardcodes frontend routes like `/applications/{id}`, coupling backend to frontend routing. Navigation is a pure UI concern — the frontend already has the `application.id` and can determine where to navigate on its own.

## What Changes

- **Remove `Redirect` property** from all command result records: `CreateApplicationResult`, `CreateAndSubmitApplicationResult`, `UpdateApplicationResult`, `UpdateAndSubmitApplicationResult`, `ApproveApplicationResult`, `RejectApplicationResult`
- **Simplify controller responses** — replace `new { application, redirect }` with `result.ToActionResult()` (consistent with existing query and submit endpoints)
- **Update frontend `ApplicationForm.svelte`** — replace `goto(result.redirect)` with client-side navigation based on `result.application.id`
- **Update existing spec** — remove redirect references from `cqrs-command-split` spec scenarios

## Capabilities

### New Capabilities

_(none — this is a cleanup/simplification)_

### Modified Capabilities

- `cqrs-command-split`: Remove redirect from command result contracts and scenario expectations. Responses return only the application object, not a redirect path.

## Impact

- **Backend Commands** (6 files): `CreateApplicationCommand.cs`, `CreateAndSubmitApplicationCommand.cs`, `UpdateApplicationCommand.cs`, `UpdateAndSubmitApplicationCommand.cs`, `ApproveApplicationCommand.cs`, `RejectApplicationCommand.cs`
- **Backend Controllers** (2 files): `ApplicationsController.cs`, `ProcessorController.cs`
- **Frontend** (1 file): `ApplicationForm.svelte`
- **Specs** (1 file): `openspec/specs/cqrs-command-split/spec.md`
- **No breaking API changes for external consumers** — the `redirect` field was only consumed by the SvelteKit frontend, which is updated in the same change
