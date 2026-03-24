## Context

The Risk Service uses a MassTransit saga (`ApplicationCreationStateMachine`) to orchestrate the Create/CreateAndSubmit application flows asynchronously: FetchCustomerProfile → CreditCheck → FinalizeApplication. However, the Update, UpdateAndSubmit, and Submit flows bypass this saga and instead call `ICustomerProfileService` (HTTP to Customer Service) and `ICreditCheckService` synchronously within the request handler. This creates direct runtime coupling between bounded contexts and means submissions may use stale credit data.

The existing saga infrastructure (state machine, consumers, fault handling, retry policies) is mature and well-tested. The Customer Read Model (populated via domain events) already replaced the `ICustomerNameService` HTTP dependency for queries, making that interface dead code.

## Goals / Non-Goals

**Goals:**
- Remove all synchronous HTTP dependencies from Risk Service command handlers to Customer Service
- Route Update, UpdateAndSubmit, and Submit flows through the existing saga pipeline
- Ensure every submission uses freshly fetched customer profile and credit check data
- Clean up dead code (`ICustomerNameService`)

**Non-Goals:**
- Changing the Create/CreateAndSubmit saga flow (already correct)
- Removing `CustomerReadModelSyncService` bootstrap HTTP sync (acceptable cold-start mechanism)
- Removing `ICustomerProfileService` entirely (still needed by `FetchCustomerProfileConsumer`)
- Changing frontend behavior beyond handling Processing status on Update/Submit (same as Create already does)
- Introducing a separate saga state machine for updates (Option B — rejected)

## Decisions

### Decision 1: Extend existing saga with OperationType field

Add an `OperationType` string property to `ApplicationCreationState` to distinguish Create vs Update flows. The state machine gets a second `Initially` block for `ApplicationUpdateStarted` events.

**Rationale**: The pipeline steps (FetchCustomerProfile → CreditCheck → Finalize) are identical for both flows. Only the final step differs (Finalize vs FinalizeUpdate). Extending one saga avoids duplicating the state machine, state persistence, and retry/fault handling infrastructure.

**Alternatives considered**:
- **Separate saga (Option B)**: Cleaner separation but duplicates all shared steps, fault consumers, and state persistence. More infrastructure to maintain for no behavioral benefit.

### Decision 2: Branch at CheckingCredit → Finalizing transition

In `During(CheckingCredit, When(CreditCheckCompleted))`, the state machine publishes either `FinalizeApplication` (existing, for Create) or `FinalizeApplicationUpdate` (new, for Update/Submit) based on `OperationType`.

**Rationale**: Keeps the finalize consumers focused on one concern each. The Create finalize calls `application.Finalize()`, the Update finalize calls `application.UpdateDetails()` + optionally `Submit()`. Mixing both in one consumer would require excessive branching.

### Decision 3: Reuse UpdateDetails() for Submit-only flow

When a user submits without changing fields, the handler passes the application's existing values (Income, FixedCosts, DesiredRate, CustomerId) into the saga event. The `FinalizeApplicationUpdateConsumer` calls `UpdateDetails()` with those same values plus fresh external data (EmploymentStatus, CreditReport), then calls `Submit()`.

**Rationale**: Avoids introducing a new domain method like `RefreshExternalData()`. The `UpdateDetails()` method already handles the scoring recalculation with external data. Passing unchanged financial values is semantically a no-op for those fields but ensures the credit report and employment status are always fresh.

### Decision 4: Remove ICustomerNameService (dead code)

Delete `ICustomerNameService` interface, remove it from `CustomerServiceClient`, and remove the `AddHttpClient<ICustomerNameService>` registration from DI. All query handlers already use `ICustomerReadModelRepository.GetCustomerNamesAsync()`.

**Rationale**: The interface has zero consumers. Keeping dead code increases confusion about which dependency path is canonical.

### Decision 5: Handlers return Processing status immediately

After validation, handlers set the application to Processing status, save, publish the saga event, and return the application in Processing state. The saga completes asynchronously.

**Rationale**: Matches the existing Create flow pattern. The frontend already handles Processing status from Create — the same mechanism applies to Update/Submit.

## Risks / Trade-offs

- **Update/Submit becomes eventually consistent** — The handler returns before external data is fetched and applied. The frontend must handle the Processing intermediate state. **Mitigation**: This pattern already exists for Create. The frontend polling/refresh mechanism already handles it.

- **Saga state table grows** — Every Update/Submit creates a saga instance. **Mitigation**: `SetCompletedWhenFinalized()` is already configured; completed sagas are cleaned up. Volume is manageable for this demo application.

- **UpdateDetails() requires Draft status** — The domain method `UpdateDetails()` throws if `Status != Draft`. The handler sets status to Processing before the saga runs. The `FinalizeApplicationUpdateConsumer` needs the application in a state where `UpdateDetails()` is callable. **Mitigation**: Either temporarily revert to Draft in the consumer before calling UpdateDetails, or relax the guard in UpdateDetails for Processing status. Investigate which approach fits the domain model better during implementation.

- **EF migration needed** — `ApplicationCreationState` table gets an `OperationType` column. **Mitigation**: Simple additive column with default value, no data migration needed.
