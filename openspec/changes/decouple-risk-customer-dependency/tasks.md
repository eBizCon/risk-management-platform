## 1. Saga State and Events

- [ ] 1.1 Add `OperationType` string property to `ApplicationCreationState` and create EF migration for the new column
- [ ] 1.2 Create `ApplicationUpdateStarted` record in `RiskManagement.Application/Sagas/ApplicationCreation/Events/`
- [ ] 1.3 Create `FinalizeApplicationUpdate` record in `RiskManagement.Application/Sagas/ApplicationCreation/Events/`

## 2. State Machine Extension

- [ ] 2.1 Add `ApplicationUpdateStarted` event definition to `ApplicationCreationStateMachine` with `CorrelateById` and `InsertOnInitial`
- [ ] 2.2 Add second `Initially` block for `ApplicationUpdateStarted` that sets `OperationType="Update"` and transitions to `FetchingCustomer`
- [ ] 2.3 Modify `Initially` block for `ApplicationCreationStarted` to set `OperationType="Create"`
- [ ] 2.4 Modify `During(CheckingCredit)` to branch: publish `FinalizeApplication` if `OperationType=="Create"`, publish `FinalizeApplicationUpdate` if `OperationType=="Update"`

## 3. FinalizeApplicationUpdate Consumer

- [ ] 3.1 Create `FinalizeApplicationUpdateConsumer` in `RiskManagement.Infrastructure/Sagas/Consumers/` that consumes `FinalizeApplicationUpdate`, calls `UpdateDetails()` with saga data, optionally calls `Submit()`, saves, and publishes `ApplicationCreationCompleted`
- [ ] 3.2 Create `FinalizeApplicationUpdateFaultConsumer` for error handling (publishes `ApplicationCreationFailed` on fault)
- [ ] 3.3 Register both consumers in `DependencyInjection.AddMessaging()`

## 4. Simplify Command Handlers

- [ ] 4.1 Refactor `UpdateApplicationHandler`: remove `ICustomerProfileService` and `ICreditCheckService` dependencies, validate + set Processing status + publish `ApplicationUpdateStarted(AutoSubmit=false)` + return
- [ ] 4.2 Refactor `UpdateAndSubmitApplicationHandler`: remove `ICustomerProfileService` and `ICreditCheckService` dependencies, validate + set Processing status + publish `ApplicationUpdateStarted(AutoSubmit=true)` + return
- [ ] 4.3 Refactor `SubmitApplicationHandler`: remove direct `IScoringConfigRepository`/`IScoringService` dependencies, validate ownership + set Processing status + publish `ApplicationUpdateStarted(AutoSubmit=true)` with current application values + return
- [ ] 4.4 Add `IPublishEndpoint` dependency to all three refactored handlers (for publishing saga events)

## 5. Domain Model Adjustment

- [ ] 5.1 Ensure `Application.UpdateDetails()` is callable from Processing status (currently requires Draft) — adjust guard clause or add a method to transition back for saga use

## 6. Dead Code Cleanup

- [ ] 6.1 Delete `ICustomerNameService` interface from `RiskManagement.Application/Services/`
- [ ] 6.2 Remove `ICustomerNameService` implementation methods (`GetCustomerNameAsync`, `GetCustomerNamesAsync`) from `CustomerServiceClient`
- [ ] 6.3 Remove `CustomerServiceClient : ICustomerNameService` from the class declaration (keep `ICustomerProfileService`)
- [ ] 6.4 Remove `AddHttpClient<ICustomerNameService, CustomerServiceClient>` registration from `DependencyInjection.AddApplicationServices()`
- [ ] 6.5 Remove `ICustomerProfileService` and `ICreditCheckService` from handler DI registrations (they are auto-scanned, but verify no manual registrations exist)

## 7. Testing

- [ ] 7.1 Write unit tests for `FinalizeApplicationUpdateConsumer` (update only, update+submit, application not found, exception handling)
- [ ] 7.2 Write unit tests for refactored `UpdateApplicationHandler` (validates, sets Processing, publishes event)
- [ ] 7.3 Write unit tests for refactored `UpdateAndSubmitApplicationHandler` (validates, sets Processing, publishes event)
- [ ] 7.4 Write unit tests for refactored `SubmitApplicationHandler` (validates, sets Processing, publishes event with current values)
- [ ] 7.5 Verify existing `FetchCustomerProfileConsumerTests` and `PerformCreditCheckConsumerTests` still pass
- [ ] 7.6 Verify existing E2E tests pass (application create/update/submit flows)
