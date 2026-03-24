## Why

The Risk Service (RiskManagement) has direct synchronous HTTP dependencies to the Customer Service in three command handlers: `UpdateApplicationHandler`, `UpdateAndSubmitApplicationHandler`, and `SubmitApplicationHandler`. These handlers call `ICustomerProfileService` (HTTP) and `ICreditCheckService` directly within the request lifecycle. A saga pattern already exists for the Create flow (`ApplicationCreationStateMachine`), but the Update/Submit flows bypass it. This violates bounded context autonomy and creates runtime coupling. Additionally, `SubmitApplicationHandler` reuses stale credit data instead of fetching fresh data, which is incorrect — every submission should use the latest credit score and customer profile.

## What Changes

- Extend the existing `ApplicationCreationStateMachine` with an `OperationType` field to support Update/Submit flows alongside Create
- Add `ApplicationUpdateStarted` saga event to trigger the Update/Submit pipeline
- Add `FinalizeApplicationUpdateConsumer` that performs `UpdateDetails()` and optionally `Submit()` using fresh data from the saga
- Simplify `UpdateApplicationHandler`, `UpdateAndSubmitApplicationHandler`, and `SubmitApplicationHandler` to only validate, set Processing status, and publish `ApplicationUpdateStarted`
- Remove `ICustomerProfileService` and `ICreditCheckService` dependencies from all command handlers (only saga consumers use them)
- Delete `ICustomerNameService` interface and its HTTP client registration (dead code, replaced by `ICustomerReadModelRepository`)
- `CustomerReadModelSyncService` (bootstrap HTTP sync) remains as acceptable one-time cold-start mechanism

## Capabilities

### New Capabilities
- `saga-application-update`: Extends the existing application creation saga to orchestrate Update, UpdateAndSubmit, and Submit flows through the same async pipeline (FetchCustomerProfile → CreditCheck → Finalize), ensuring fresh external data on every operation

### Modified Capabilities
- `application-credit-check`: Credit check is no longer performed synchronously in handlers; all flows (Create, Update, Submit) go through the saga pipeline
- `service-communication`: Risk Service no longer makes synchronous HTTP calls to Customer Service from command handlers; only saga consumers and bootstrap sync use HTTP

## Impact

- **Backend handlers**: `UpdateApplicationHandler`, `UpdateAndSubmitApplicationHandler`, `SubmitApplicationHandler` — simplified, lose HTTP dependencies
- **Saga infrastructure**: `ApplicationCreationState` extended with `OperationType`, state machine gets second `Initially` block, new `FinalizeApplicationUpdate` event and consumer
- **DI registration**: `ICustomerNameService` HTTP client registration removed; `ICustomerProfileService` registration stays (used by saga consumer)
- **Dead code removed**: `ICustomerNameService` interface, corresponding methods in `CustomerServiceClient`
- **Database**: `ApplicationCreationState` table gets `OperationType` column (EF migration needed)
- **API behavior**: Update/Submit endpoints return immediately with Processing status; final result arrives asynchronously via saga (same pattern as Create)
- **Frontend**: May need to handle Processing status on Update/Submit responses (same as Create flow already does)
