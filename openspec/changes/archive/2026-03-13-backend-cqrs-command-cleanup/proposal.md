## Why

The DDD refactoring introduced a CQRS command/query pattern, but several handlers still use string-based branching to decide which domain operation to invoke. This violates the CQRS principle of "one command = one intention" and leads to weak type safety, mixed validation concerns, and implicit control flow via DTO properties.

## What Changes

- **Rename `MarkAsDeleted()` to `Delete()`** on the Application aggregate, adding an `ApplicationDeletedEvent`. The method becomes a proper domain operation with a side effect instead of a bare guard.
- **Split `ProcessDecisionCommand`** into `ApproveApplicationCommand` and `RejectApplicationCommand`, each with its own handler and validation rules. Eliminates string-based `Decision` dispatch.
- **Split `CreateApplicationCommand`** into `CreateApplicationCommand` (save draft) and `CreateAndSubmitApplicationCommand` (save + submit). Eliminates `Action` property branching.
- **Split `UpdateApplicationCommand`** into `UpdateApplicationCommand` (save draft) and `UpdateAndSubmitApplicationCommand` (update + submit). Same pattern.
- **Remove `ProcessorDecisionDto`**, replace with dedicated `ApproveApplicationDto` and `RejectApplicationDto` with appropriate validation (comment required only for reject).
- **Remove `Action` property** from `ApplicationCreateDto` and `ApplicationUpdateDto`.
- **Update controllers** to dispatch to the correct handler based on the endpoint/query parameter.

## Capabilities

### New Capabilities

- `cqrs-command-split`: Splitting multi-intent commands into single-intent commands with dedicated handlers and validation

### Modified Capabilities

## Impact

- **Domain Layer**: `Application.MarkAsDeleted()` renamed to `Delete()`, new `ApplicationDeletedEvent`
- **Application Layer**: 3 command files replaced by 6 (ProcessDecision → Approve + Reject, Create → Create + CreateAndSubmit, Update → Update + UpdateAndSubmit). DTOs updated. Validators updated.
- **API Layer**: `ProcessorController` and `ApplicationsController` updated to dispatch new commands. Endpoint structure may change (e.g. `/processor/{id}/approve`, `/processor/{id}/reject`).
- **Tests**: `ValidationTests.cs` updated for new DTOs/validators. New unit tests for split handlers.
- **Frontend**: API calls may need adjustment if endpoint paths change. Alternatively, controller can keep existing endpoints and dispatch internally.
