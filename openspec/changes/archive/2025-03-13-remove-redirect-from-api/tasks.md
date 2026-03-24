## 1. Backend: Remove Redirect from Result Records

- [x] 1.1 `CreateApplicationCommand.cs`: Change `CreateApplicationResult(ApplicationResponse Application, string Redirect)` to `CreateApplicationResult(ApplicationResponse Application)`. Remove redirect string from `Result.Success(...)` call in handler.
- [x] 1.2 `CreateAndSubmitApplicationCommand.cs`: Change `CreateAndSubmitApplicationResult(ApplicationResponse Application, string Redirect)` to `CreateAndSubmitApplicationResult(ApplicationResponse Application)`. Remove redirect string from `Result.Success(...)` call in handler.
- [x] 1.3 `UpdateApplicationCommand.cs`: Change `UpdateApplicationResult(ApplicationResponse Application, string Redirect)` to `UpdateApplicationResult(ApplicationResponse Application)`. Remove redirect string from `Result.Success(...)` call in handler.
- [x] 1.4 `UpdateAndSubmitApplicationCommand.cs`: Change `UpdateAndSubmitApplicationResult(ApplicationResponse Application, string Redirect)` to `UpdateAndSubmitApplicationResult(ApplicationResponse Application)`. Remove redirect string from `Result.Success(...)` call in handler.
- [x] 1.5 `ApproveApplicationCommand.cs`: Change `ApproveApplicationResult(ApplicationResponse Application, string Redirect)` to `ApproveApplicationResult(ApplicationResponse Application)`. Remove redirect string from `Result.Success(...)` call in handler.
- [x] 1.6 `RejectApplicationCommand.cs`: Change `RejectApplicationResult(ApplicationResponse Application, string Redirect)` to `RejectApplicationResult(ApplicationResponse Application)`. Remove redirect string from `Result.Success(...)` call in handler.

## 2. Backend: Simplify Controller Responses

- [x] 2.1 `ApplicationsController.cs`: Replace `return Ok(new { application = ..., redirect = ... })` with `return submitResult.ToActionResult()` / `return result.ToActionResult()` in `CreateApplication` and `UpdateApplication` methods. Remove manual `IsSuccess` checks that are now handled by `ToActionResult()`.
- [x] 2.2 `ProcessorController.cs`: Replace `return Ok(new { application = ..., redirect = ... })` with `return result.ToActionResult()` in `ApproveApplication` and `RejectApplication` methods. Remove manual `IsSuccess` checks.

## 3. Frontend: Client-side Navigation

- [x] 3.1 `ApplicationForm.svelte`: Replace `if (result.redirect) { await goto(result.redirect); }` with navigation based on `result.application.id`, e.g. `await goto(\`/applications/${result.application.id}\`)`.

## 4. Spec Update

- [x] 4.1 Update `openspec/specs/cqrs-command-split/spec.md`: Remove redirect references from scenarios for create-and-submit, update-and-submit, create, and update commands. Responses contain only the application object.

## 5. Verification

- [x] 5.1 Verify backend builds without errors (`dotnet build`)
- [x] 5.2 Verify backend unit tests pass (`dotnet test`)
- [x] 5.3 Verify frontend builds without errors (`npm run build`)
- [ ] 5.4 Verify E2E tests pass (`npm run test:e2e:ci`) — blocked: backend API not running (pre-existing infra issue, not related to this change)
