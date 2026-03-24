## 1. Domain Layer — Delete Operation

- [x] 1.1 Rename `MarkAsDeleted()` to `Delete()` on `Application` aggregate, keep the Draft guard
- [x] 1.2 Create `ApplicationDeletedEvent` implementing `IDomainEvent` (contains ApplicationId)
- [x] 1.3 Add `AddDomainEvent(new ApplicationDeletedEvent(Id))` inside `Delete()`
- [x] 1.4 Update `DeleteApplicationHandler` to call `application.Delete()` instead of `application.MarkAsDeleted()`

## 2. Application Layer — Split ProcessDecision into Approve + Reject

- [x] 2.1 Create `ApproveApplicationDto` with optional `Comment` property
- [x] 2.2 Create `RejectApplicationDto` with required `Comment` property
- [x] 2.3 Create `ApproveApplicationCommand(int ApplicationId, ApproveApplicationDto Dto)` and `ApproveApplicationHandler` — validates, loads aggregate, calls `Approve(comment)`, saves
- [x] 2.4 Create `RejectApplicationCommand(int ApplicationId, RejectApplicationDto Dto)` and `RejectApplicationHandler` — validates, loads aggregate, calls `Reject(comment)`, saves
- [x] 2.5 Create `ApproveApplicationValidator` (comment optional, simple rules)
- [x] 2.6 Create `RejectApplicationValidator` (comment required, `NotEmpty` rule)
- [x] 2.7 Delete `ProcessDecisionCommand.cs`, `ProcessorDecisionDto`, and `ProcessorDecisionValidator`

## 3. Application Layer — Split Create/Update + Submit

- [x] 3.1 Remove `Action` property from `ApplicationCreateDto` and `ApplicationUpdateDto`
- [x] 3.2 Create `CreateAndSubmitApplicationCommand` and `CreateAndSubmitApplicationHandler` — creates aggregate, submits, saves in one transaction
- [x] 3.3 Create `UpdateAndSubmitApplicationCommand` and `UpdateAndSubmitApplicationHandler` — loads aggregate, updates, submits, saves in one transaction
- [x] 3.4 Remove `if (command.Dto.Action == "submit")` branch from `CreateApplicationHandler` — handler only saves draft
- [x] 3.5 Remove `if (command.Dto.Action == "submit")` branch from `UpdateApplicationHandler` — handler only saves draft

## 4. Application Layer — Extract ToCamelCase Utility

- [x] 4.1 Create `ValidationHelper` static class in `Application/Common/` with `ToCamelCase` and `ToValidationErrors` methods
- [x] 4.2 Update all handlers to use `ValidationHelper` instead of private `ToCamelCase` methods
- [x] 4.3 Remove duplicate `ToCamelCase` methods from individual handlers

## 5. API Layer — Update Controllers

- [x] 5.1 Update `ProcessorController`: replace `POST {id}/decide` endpoint with `POST {id}/approve` and `POST {id}/reject`, inject new handlers
- [x] 5.2 Update `ApplicationsController`: read `submit` query parameter in `CreateApplication` and `UpdateApplication`, dispatch to appropriate handler
- [x] 5.3 Remove old handler DI registrations, add new ones in `DependencyInjection.cs`

## 6. Frontend — Adapt API Calls

- [x] 6.1 Update processor decision page: change `fetch(\`/api/processor/${id}/decide\`)` to `fetch(\`/api/processor/${id}/${selectedDecision}\`)`, remove `decision` from body
- [x] 6.2 Update `ApplicationForm.svelte`: change `submitForm('submit')` to append `?submit=true` query parameter to the fetch URL, remove `action` from request body

## 7. Tests — Update and Verify

- [x] 7.1 Update `ValidationTests.cs`: replace `ProcessorDecisionValidator` tests with `ApproveApplicationValidator` and `RejectApplicationValidator` tests
- [x] 7.2 Update `ValidationTests.cs`: remove tests referencing `Action` property on DTOs
- [x] 7.3 Verify solution builds without errors (`dotnet build`)
- [x] 7.4 Verify all unit tests pass (`dotnet test`)
- [ ] 7.5 Verify E2E tests pass (`npm run test:e2e:ci`)
