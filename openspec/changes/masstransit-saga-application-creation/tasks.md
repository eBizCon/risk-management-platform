## 1. Infrastructure & NuGet Setup

- [x] 1.1 Add NuGet packages to RiskManagement.Infrastructure: `MassTransit`, `MassTransit.RabbitMQ`, `MassTransit.EntityFrameworkCore`
- [x] 1.2 Add NuGet package to AppHost: `Aspire.Hosting.RabbitMQ`
- [x] 1.3 Add RabbitMQ resource to `AppHost/Program.cs`: `builder.AddRabbitMQ("messaging").WithDataVolume()`, wire `WithReference(rabbitmq)` and `WaitFor(rabbitmq)` to riskApi
- [x] 1.4 Add RabbitMQ service to `dev/docker-compose.yml` with ports 5672/15672, management plugin, and data volume
- [x] 1.5 Add RabbitMQ connection fallback to `appsettings.Development.json` for non-Aspire development

## 2. Domain: ApplicationStatus Extensions

- [x] 2.1 Add `ApplicationStatus.Processing` and `ApplicationStatus.Failed` to `ApplicationStatus.cs` with corresponding dictionary entries
- [x] 2.2 Add `FailureReason` (string?) property to the `Application` aggregate
- [x] 2.3 Add `CreateProcessing()` factory method to `Application` aggregate: accepts CustomerId, Income, FixedCosts, DesiredRate, CreatedBy; sets Status=Processing, CreditReport=null, EmploymentStatus default
- [x] 2.4 Add `Finalize()` method to `Application` aggregate: guards Status==Processing, accepts EmploymentStatus, CreditReport, IScoringService, ScoringConfig, ScoringConfigVersionId; sets all data, applies scoring, transitions to Draft
- [x] 2.5 Add `MarkFailed(string reason)` method to `Application` aggregate: guards Status==Processing, sets FailureReason, transitions to Failed
- [x] 2.6 Update `Submit()`, `Approve()`, `Reject()` guards to reject transitions from `Processing` and `Failed` status
- [x] 2.7 Update `Delete()` to also allow deletion of `Failed` applications (in addition to Draft)

## 3. Database: EF Core Migration

- [x] 3.1 Add `FailureReason` column mapping to `ApplicationConfiguration.cs`
- [x] 3.2 Create `ApplicationCreationStateMap` SagaClassMap for `ApplicationCreationState` (CorrelationId as PK, all state fields mapped)
- [x] 3.3 Register `DbSet<ApplicationCreationState>` in `ApplicationDbContext` and add saga entity configuration
- [x] 3.4 Create EF Core migration for new ApplicationStatus values, FailureReason column, and saga_application_creation_state table

## 4. Saga Messages

- [x] 4.1 Create message records in `RiskManagement.Application/Sagas/ApplicationCreation/Events/`: `ApplicationCreationStarted`, `FetchCustomerProfile`, `CustomerProfileFetched`, `PerformCreditCheck`, `CreditCheckCompleted`, `FinalizeApplication` (fat message with all data + AutoSubmit flag), `ApplicationCreationCompleted`, `ApplicationCreationFailed`

## 5. Saga State & State Machine

- [x] 5.1 Create `ApplicationCreationState` class implementing `SagaStateMachineInstance` with all fields (CorrelationId, CurrentState, input data, intermediate results, AutoSubmit, FailureReason, CreatedAt, CompletedAt)
- [x] 5.2 Create `ApplicationCreationStateMachine` with states (FetchingCustomer, CheckingCredit, Finalizing, Completed, Failed), event correlations, and state transitions as defined in spec
- [x] 5.3 Configure `InsertOnInitial` and `SetSagaFactory` for the `ApplicationCreationStarted` event
- [x] 5.4 Configure `SetCompletedWhenFinalized()` to clean up completed/failed saga instances

## 6. Saga Consumers

- [x] 6.1 Implement `FetchCustomerProfileConsumer`: inject `ICustomerProfileService`, fetch profile, publish `CustomerProfileFetched` on success or `ApplicationCreationFailed` on not-found
- [x] 6.2 Implement `PerformCreditCheckConsumer`: inject `ICreditCheckService`, perform check, publish `CreditCheckCompleted`
- [x] 6.3 Implement `FinalizeApplicationConsumer`: inject `IApplicationRepository`, `IScoringConfigRepository`, `IScoringService`; load application, call `Finalize()` (+ `Submit()` if AutoSubmit), save, publish `ApplicationCreationCompleted`; on failure publish `ApplicationCreationFailed` and call `MarkFailed()`

## 7. MassTransit Registration

- [x] 7.1 Create `AddMessaging()` extension method in `DependencyInjection.cs`: register saga state machine with EF Core PostgreSQL repository, register all consumers, configure RabbitMQ transport with retry policy (1s, 5s, 15s, 30s), set kebab-case endpoint name formatter
- [x] 7.2 Wire `AddMessaging()` call in `RiskManagement.Api/Program.cs` with RabbitMQ connection string from Aspire configuration
- [x] 7.3 Add `IPublishEndpoint` injection capability for command handlers (provided automatically by MassTransit DI)

## 8. Command Handler Refactoring

- [x] 8.1 Refactor `CreateApplicationHandler`: inject `IPublishEndpoint`, create Processing application via `Application.CreateProcessing()`, save, publish `ApplicationCreationStarted` event, return result with 202 status
- [x] 8.2 Refactor `CreateAndSubmitApplicationHandler`: same as 8.1 but set AutoSubmit=true in the `ApplicationCreationStarted` event
- [x] 8.3 Update `ApplicationsController` to return `Accepted()` (HTTP 202) for create/create-and-submit commands instead of `Ok()`
- [x] 8.4 Verify `UpdateApplicationHandler` and `UpdateAndSubmitApplicationHandler` remain unchanged (synchronous flow)

## 9. Frontend: Processing State & Polling

- [x] 9.1 Add "processing" and "failed" status handling to the application status display (badge colors, labels)
- [x] 9.2 Implement polling logic after receiving 202 response: poll `GET /api/applications/{id}` every 2 seconds until status != "processing"
- [x] 9.3 Add processing UI: loading indicator with "Antrag wird verarbeitet..." text during polling
- [x] 9.4 Add failure UI: error message with failure reason, "Erneut versuchen" (retry) and "Löschen" (delete) buttons
- [x] 9.5 Update application creation form to handle 202 response (redirect to detail page with polling instead of immediate display)

## 10. Tests

- [x] 10.1 Unit tests for `Application.CreateProcessing()`: verify Status=Processing, null CreditReport, null Score
- [x] 10.2 Unit tests for `Application.Finalize()`: verify transitions Processing→Draft, sets EmploymentStatus/CreditReport/Score, throws on non-Processing
- [x] 10.3 Unit tests for `Application.MarkFailed()`: verify transitions Processing→Failed, stores reason, throws on non-Processing
- [x] 10.4 Unit tests for status transition guards: Processing cannot Submit/Approve/Reject; Failed can Delete
- [x] 10.5 Unit tests for `FetchCustomerProfileConsumer`: mock ICustomerProfileService, verify publishes CustomerProfileFetched or ApplicationCreationFailed
- [x] 10.6 Unit tests for `PerformCreditCheckConsumer`: mock ICreditCheckService, verify publishes CreditCheckCompleted
- [x] 10.7 Unit tests for `FinalizeApplicationConsumer`: mock repositories + scoring, verify loads application, calls Finalize(), saves, publishes ApplicationCreationCompleted; verify MarkFailed on error
- [x] 10.8 Integration test for `ApplicationCreationStateMachine`: use MassTransit InMemory test harness, verify full state flow from ApplicationCreationStarted through Completed
- [x] 10.9 Integration test for saga failure path: verify ApplicationCreationFailed transitions saga to Failed state
- [x] 10.10 Verify existing E2E tests still pass (update flows, approve, reject, delete remain synchronous)
