# Application Handling — Lifecycle & Message Patterns

## Overview

The **Application** aggregate is the central domain object of the RiskManagement bounded context. It represents a credit/risk application submitted by an applicant, processed through scoring, and decided upon by a processor. The lifecycle involves synchronous CQRS commands/queries via a custom Dispatcher, asynchronous orchestration via a MassTransit Saga (state machine), and cross-bounded-context integration via RabbitMQ integration events.

---

## 1. Domain Model

### Aggregate: `Application`

| Property               | Type                     | Description                                    |
|------------------------|--------------------------|------------------------------------------------|
| `Id`                   | `ApplicationId`          | Strongly typed ID (int)                        |
| `CustomerId`           | `int`                    | FK to Customer (cross-BC, via read model)      |
| `Income`               | `Money`                  | Monthly income                                 |
| `FixedCosts`           | `Money`                  | Monthly fixed costs                            |
| `DesiredRate`          | `Money`                  | Desired monthly repayment rate                 |
| `EmploymentStatus`     | `EmploymentStatus`       | Enumeration value object                       |
| `CreditReport`         | `CreditReport`           | Value object (SCHUFA data)                     |
| `Status`               | `ApplicationStatus`      | Current lifecycle status                       |
| `Score`                | `int?`                   | Calculated risk score                          |
| `TrafficLight`         | `TrafficLight?`          | Green / Yellow / Red                           |
| `ScoringReasons`       | `string?`                | JSON-serialized scoring breakdown              |
| `ScoringConfigVersionId` | `ScoringConfigVersionId?` | Which config version was used for scoring    |
| `ProcessorComment`     | `string?`                | Comment from processor on approve/reject       |
| `FailureReason`        | `string?`                | Reason if creation saga failed                 |
| `CreatedAt`            | `DateTime`               | Creation timestamp                             |
| `SubmittedAt`          | `DateTime?`              | Submission timestamp                           |
| `ProcessedAt`          | `DateTime?`              | Approval/rejection timestamp                   |
| `CreatedBy`            | `EmailAddress`           | Email of the creating applicant                |
| `Inquiries`            | `IReadOnlyList<ApplicationInquiry>` | Child entity collection          |

### Child Entity: `ApplicationInquiry`

Represents a back-and-forth information request between processor and applicant.

| Property         | Type            | Description                      |
|------------------|-----------------|----------------------------------|
| `Id`             | `InquiryId`     | Strongly typed ID                |
| `ApplicationId`  | `ApplicationId` | Parent reference                 |
| `InquiryText`    | `string`        | Question from processor          |
| `ResponseText`   | `string?`       | Answer from applicant            |
| `Status`         | `InquiryStatus` | Open / Answered                  |
| `ProcessorEmail` | `EmailAddress`  | Who asked                        |
| `CreatedAt`      | `DateTime`      | When the inquiry was created     |
| `AnsweredAt`     | `DateTime?`     | When the applicant responded     |

---

## 2. Application Status Lifecycle

```
                  ┌──────────────────────────────────────────────────────┐
                  │                                                      │
                  ▼                                                      │
┌──────────┐  Saga completes   ┌───────┐  Submit()   ┌───────────┐      │
│Processing │ ───────────────► │ Draft │ ──────────► │ Submitted │      │
└──────────┘   Finalize()      └───────┘              └─────┬─────┘      │
     │                            │                         │            │
     │ MarkFailed()               │ Delete()                ├── Approve() ──► ┌──────────┐
     ▼                            ▼                         │                 │ Approved │
┌────────┐                   (removed)                      │                 └──────────┘
│ Failed │                                                  │
└────────┘                                                  ├── Reject() ───► ┌──────────┐
     │                                                      │                 │ Rejected │
     │ Delete()                                             │                 └──────────┘
     ▼                                                      │
  (removed)                                                 │ RequestInformation()
                                                            ▼
                                                ┌───────────────────────┐
                                                │  NeedsInformation     │
                                                └───────────┬───────────┘
                                                            │
                                                            │ AnswerInquiry()
                                                            ▼
                                                ┌───────────────────────┐
                                                │    Resubmitted        │──── Approve() / Reject()
                                                └───────────────────────┘     (same as Submitted)
```

### Status Values (`ApplicationStatus` — Enumeration Value Object)

| Status               | Value                | Description                                           |
|----------------------|----------------------|-------------------------------------------------------|
| `Processing`         | `processing`         | Saga is running (customer fetch + credit check)       |
| `Draft`              | `draft`              | Saga completed; application editable by applicant     |
| `Submitted`          | `submitted`          | Applicant submitted; awaiting processor decision      |
| `NeedsInformation`   | `needs_information`  | Processor requested additional information            |
| `Resubmitted`        | `resubmitted`        | Applicant answered inquiry; back in processor queue   |
| `Approved`           | `approved`           | Processor approved the application                    |
| `Rejected`           | `rejected`           | Processor rejected the application                    |
| `Failed`             | `failed`             | Saga failed (customer not found, scoring error, etc.) |

### Allowed Status Transitions

| From              | To                  | Method                 | Guard                                             |
|-------------------|---------------------|------------------------|----------------------------------------------------|
| Processing        | Draft               | `Finalize()`           | Status == Processing                               |
| Processing        | Failed              | `MarkFailed()`         | Status == Processing                               |
| Draft             | Submitted           | `Submit()`             | Status == Draft                                    |
| Draft             | *(deleted)*         | `Delete()`             | Status == Draft                                    |
| Failed            | *(deleted)*         | `Delete()`             | Status == Failed                                   |
| Draft             | Draft               | `UpdateDetails()`      | Status == Draft (re-score, no transition)          |
| Submitted         | Approved            | `Approve()`            | Status == Submitted or Resubmitted                 |
| Submitted         | Rejected            | `Reject()`             | Status == Submitted or Resubmitted                 |
| Submitted         | NeedsInformation    | `RequestInformation()` | Status == Submitted or Resubmitted; no open inquiry|
| Resubmitted       | Approved            | `Approve()`            | Status == Submitted or Resubmitted                 |
| Resubmitted       | Rejected            | `Reject()`             | Status == Submitted or Resubmitted                 |
| Resubmitted       | NeedsInformation    | `RequestInformation()` | Status == Submitted or Resubmitted; no open inquiry|
| NeedsInformation  | Resubmitted         | `AnswerInquiry()`      | Status == NeedsInformation; open inquiry exists    |

Invalid transitions throw `InvalidStatusTransitionException` or `DomainException`.

---

## 3. CQRS — Commands

All commands are dispatched via `IDispatcher.SendAsync()`. Handlers implement `ICommandHandler<TCommand, TResult>` and return `Result<T>`.

### Application Commands

| Command                          | Trigger                          | Effect                                                                                 |
|----------------------------------|----------------------------------|----------------------------------------------------------------------------------------|
| `CreateApplicationCommand`       | `POST /api/applications`         | Creates application in `Processing` status; publishes `ApplicationCreationStarted` to RabbitMQ (AutoSubmit=false) |
| `CreateAndSubmitApplicationCommand` | `POST /api/applications?submit=true` | Same as above but with `AutoSubmit=true` — saga will auto-submit after finalization |
| `UpdateApplicationCommand`       | `PUT /api/applications/{id}`     | Updates draft application details; re-fetches customer profile & credit report; re-scores |
| `UpdateAndSubmitApplicationCommand` | `PUT /api/applications/{id}?submit=true` | Updates + submits in one operation; raises `ApplicationSubmittedEvent`        |
| `SubmitApplicationCommand`       | `POST /api/applications/{id}/submit` | Submits a draft application; re-scores; raises `ApplicationSubmittedEvent`         |
| `DeleteApplicationCommand`       | `DELETE /api/applications/{id}`  | Deletes draft or failed application; raises `ApplicationDeletedEvent`                  |
| `ApproveApplicationCommand`      | `POST /api/processor/{id}/approve` | Approves submitted/resubmitted application; raises `ApplicationDecidedEvent("approved")` |
| `RejectApplicationCommand`       | `POST /api/processor/{id}/reject`  | Rejects submitted/resubmitted application; raises `ApplicationDecidedEvent("rejected")` |
| `CreateInquiryCommand`           | `POST /api/applications/{id}/inquiry` | Processor creates inquiry; transitions to `NeedsInformation`; raises `InquiryCreatedEvent` |
| `AnswerInquiryCommand`           | `POST /api/applications/{id}/inquiry/response` | Applicant answers inquiry; transitions to `Resubmitted`               |
| `RescoreOpenApplicationsCommand` | *(triggered after scoring config update)* | Re-scores all open (non-decided) applications with latest scoring config   |
| `UpdateScoringConfigCommand`     | *(processor endpoint)*           | Creates new `ScoringConfigVersion`; not directly an Application command but triggers rescore |

### Authorization

| Controller                 | Policy              | Role        |
|----------------------------|----------------------|-------------|
| `ApplicationsController`  | `Applicant`          | applicant   |
| `ProcessorController`     | `Processor`          | processor   |
| `InquiryController`       | Mixed                | both roles  |
| `InternalApplicationsController` | *(none — internal)* | service-to-service |

---

## 4. CQRS — Queries

All queries are dispatched via `IDispatcher.QueryAsync()`. Handlers implement `IQueryHandler<TQuery, TResult>`.

| Query                         | Trigger                                      | Returns                                     |
|-------------------------------|----------------------------------------------|---------------------------------------------|
| `GetApplicationsByUserQuery`  | `GET /api/applications`                      | Applications for the logged-in applicant    |
| `GetApplicationQuery`         | `GET /api/applications/{id}`                 | Single application (role-aware access)      |
| `GetProcessorApplicationsQuery` | `GET /api/processor/applications`          | Paginated list for processor view           |
| `GetDashboardStatsQuery`      | *(dashboard endpoint)*                       | Counts by status (Draft/Submitted/Approved/Rejected) |
| `GetInquiriesQuery`           | `GET /api/applications/{id}/inquiries`       | Inquiries for an application                |
| `GetActiveCustomersQuery`     | `GET /api/applications/customers`            | Active customers (from CustomerReadModel)   |
| `GetScoringConfigQuery`       | *(scoring config endpoint)*                  | Current scoring configuration               |
| `CheckApplicationsExistQuery` | `GET /api/internal/applications/exists?customerId=` | Whether applications exist for a customer (internal API) |

---

## 5. Domain Events (In-Process)

Domain events are raised on the aggregate via `AddDomainEvent()` and dispatched **after save** via `IDispatcher.PublishDomainEventsAsync(aggregate)`. They are handled by `IDomainEventHandler<T>` implementations (in-process, same transaction boundary).

| Event                      | Raised By                 | Payload                                  |
|----------------------------|---------------------------|------------------------------------------|
| `ApplicationSubmittedEvent`| `Submit()`                | `ApplicationId`, `OccurredOn`            |
| `ApplicationDecidedEvent`  | `Approve()` / `Reject()`  | `ApplicationId`, `Decision` ("approved"/"rejected"), `OccurredOn` |
| `ApplicationDeletedEvent`  | `Delete()`                | `ApplicationId`, `OccurredOn`            |
| `InquiryCreatedEvent`      | `RequestInformation()`    | `ApplicationId`, `InquiryId`, `OccurredOn` |

> **Note:** Currently no `IDomainEventHandler` implementations are registered. The events are raised and dispatched but silently ignored (the Dispatcher tolerates zero handlers). They serve as extension points for future side effects (e.g., notifications, audit logging).

---

## 6. Application Creation Saga (MassTransit State Machine)

The creation of an application is orchestrated asynchronously via a **MassTransit Saga State Machine**. This decouples the HTTP request from long-running steps (cross-BC customer profile fetch, external credit check, scoring).

### Saga State Machine: `ApplicationCreationStateMachine`

**State Machine States:**

```
Initial ──► FetchingCustomer ──► CheckingCredit ──► Finalizing ──► Completed
                                                                       │
                        ◄──────── Failed ◄─── (from any state) ────────┘
```

| State              | Description                                               |
|--------------------|-----------------------------------------------------------|
| `Initial`          | Saga not yet started                                      |
| `FetchingCustomer` | Waiting for customer profile from CustomerManagement BC   |
| `CheckingCredit`   | Waiting for SCHUFA credit check result                    |
| `Finalizing`       | Waiting for application finalization (scoring + persist)   |
| `Completed`        | Saga finished successfully                                |
| `Failed`           | Saga failed at any step                                   |

### Saga Messages (via RabbitMQ)

```
┌─────────────────────────┐
│ CreateApplication-       │
│ Command (HTTP Handler)   │
│                          │
│ 1. Create Application    │
│    (Status: Processing)  │
│ 2. Publish:              │
│    ApplicationCreation-  │
│    Started               │
└──────────┬──────────────┘
           │
           ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                    APPLICATION CREATION SAGA                            │
│                                                                         │
│  ApplicationCreationStarted                                             │
│  │  Store saga state, publish:                                          │
│  ▼                                                                      │
│  FetchCustomerProfile ──► FetchCustomerProfileConsumer                  │
│  │                         (calls CustomerManagement API)               │
│  │                         publishes: CustomerProfileFetched            │
│  ▼                                    or ApplicationCreationFailed      │
│  CustomerProfileFetched                                                 │
│  │  Store profile data, publish:                                        │
│  ▼                                                                      │
│  PerformCreditCheck ──► PerformCreditCheckConsumer                      │
│  │                       (calls ICreditCheckService / MockSchufa)       │
│  │                       publishes: CreditCheckCompleted                │
│  ▼                                                                      │
│  CreditCheckCompleted                                                   │
│  │  Store credit data, publish:                                         │
│  ▼                                                                      │
│  FinalizeApplication ──► FinalizeApplicationConsumer                    │
│  │                        - Loads Application from DB                   │
│  │                        - Calls Application.Finalize()                │
│  │                          (Processing → Draft)                        │
│  │                        - If AutoSubmit: Application.Submit()          │
│  │                          (Draft → Submitted)                         │
│  │                        - SaveChanges                                 │
│  │                        publishes: ApplicationCreationCompleted       │
│  │                                   or ApplicationCreationFailed       │
│  ▼                                                                      │
│  ApplicationCreationCompleted → Saga transitions to Completed           │
│                                                                         │
│  ─── At any point ───                                                   │
│  ApplicationCreationFailed → Saga transitions to Failed                 │
│                               Application.MarkFailed(reason)            │
└─────────────────────────────────────────────────────────────────────────┘
```

### Saga Messages Reference

| Message                        | Type     | Publisher                          | Consumer / Handler                    | Payload                                                                 |
|-------------------------------|----------|------------------------------------|---------------------------------------|-------------------------------------------------------------------------|
| `ApplicationCreationStarted`  | Event    | Command Handler (via MassTransit)  | Saga State Machine                    | CorrelationId, ApplicationId, CustomerId, Income, FixedCosts, DesiredRate, UserEmail, AutoSubmit |
| `FetchCustomerProfile`        | Command  | Saga State Machine                 | `FetchCustomerProfileConsumer`        | CorrelationId, CustomerId                                               |
| `CustomerProfileFetched`      | Event    | `FetchCustomerProfileConsumer`     | Saga State Machine                    | CorrelationId, FirstName, LastName, EmploymentStatus, DateOfBirth, Street, City, ZipCode, Country |
| `PerformCreditCheck`          | Command  | Saga State Machine                 | `PerformCreditCheckConsumer`          | CorrelationId, FirstName, LastName, DateOfBirth, Street, City, ZipCode, Country |
| `CreditCheckCompleted`        | Event    | `PerformCreditCheckConsumer`       | Saga State Machine                    | CorrelationId, HasPaymentDefault, CreditScore, CheckedAt, Provider      |
| `FinalizeApplication`         | Command  | Saga State Machine                 | `FinalizeApplicationConsumer`         | CorrelationId, ApplicationId, CustomerId, Income, FixedCosts, DesiredRate, UserEmail, EmploymentStatus, HasPaymentDefault, CreditScore, CreditCheckedAt, CreditProvider, AutoSubmit |
| `ApplicationCreationCompleted`| Event    | `FinalizeApplicationConsumer`      | Saga State Machine                    | CorrelationId                                                           |
| `ApplicationCreationFailed`   | Event    | Any Consumer (on error)            | Saga State Machine                    | CorrelationId, Reason                                                   |

### Saga Persistence

- **Store:** EF Core (`ApplicationDbContext`) with PostgreSQL
- **Correlation:** All messages carry `CorrelationId` (Guid) for saga instance matching
- **Retry Policy:** 1s → 5s → 15s → 30s intervals
- **Completion:** `SetCompletedWhenFinalized()` — saga instance is removed from DB after reaching `Completed` or `Failed`

---

## 7. Cross-Bounded-Context Integration Events (RabbitMQ)

The RiskManagement BC consumes integration events published by the **CustomerManagement BC** to keep a local `CustomerReadModel` in sync. These are **not** domain events — they are integration events defined in the `SharedKernel`.

| Integration Event                        | Consumer                       | Effect                                        |
|------------------------------------------|--------------------------------|-----------------------------------------------|
| `CustomerCreatedIntegrationEvent`        | `CustomerCreatedConsumer`      | Upsert `CustomerReadModel` (id, name, status) |
| `CustomerUpdatedIntegrationEvent`        | `CustomerUpdatedConsumer`      | Update name + status on existing read model    |
| `CustomerActivatedIntegrationEvent`      | `CustomerActivatedConsumer`    | Set status = "active"                          |
| `CustomerArchivedIntegrationEvent`       | `CustomerArchivedConsumer`     | Set status = "archived"                        |
| `CustomerDeletedIntegrationEvent`        | `CustomerDeletedConsumer`      | Remove `CustomerReadModel` from DB             |

Additionally, a `CustomerReadModelSyncService` (BackgroundService) performs an initial full sync from the CustomerManagement API on startup if the read model table is empty.

---

## 8. Infrastructure & Messaging Configuration

### Transport

- **Message Broker:** RabbitMQ
- **Library:** MassTransit
- **Endpoint Naming:** Kebab-case (`set-kebab-case-endpoint-name-formatter`)
- **Retry:** Exponential intervals (1s, 5s, 15s, 30s)

### Registered Consumers (in `DependencyInjection.AddMessaging()`)

| Consumer                          | Message Type                            | Category              |
|-----------------------------------|-----------------------------------------|-----------------------|
| `FetchCustomerProfileConsumer`    | `FetchCustomerProfile`                  | Saga step             |
| `PerformCreditCheckConsumer`      | `PerformCreditCheck`                    | Saga step             |
| `FinalizeApplicationConsumer`     | `FinalizeApplication`                   | Saga step             |
| `CustomerCreatedConsumer`         | `CustomerCreatedIntegrationEvent`       | Cross-BC sync         |
| `CustomerUpdatedConsumer`         | `CustomerUpdatedIntegrationEvent`       | Cross-BC sync         |
| `CustomerActivatedConsumer`       | `CustomerActivatedIntegrationEvent`     | Cross-BC sync         |
| `CustomerArchivedConsumer`        | `CustomerArchivedIntegrationEvent`      | Cross-BC sync         |
| `CustomerDeletedConsumer`         | `CustomerDeletedIntegrationEvent`       | Cross-BC sync         |

### CQRS Dispatcher (In-Process)

- **Interface:** `IDispatcher` with `SendAsync`, `QueryAsync`, `PublishAsync`, `PublishDomainEventsAsync`
- **Implementation:** Reflection-based handler resolution via `IServiceProvider`
- **Registration:** Assembly scanning auto-registers all `ICommandHandler<,>`, `IQueryHandler<,>`, `IDomainEventHandler<>` as scoped

---

## 9. Key File Locations

| Concern                     | Path                                                                  |
|-----------------------------|-----------------------------------------------------------------------|
| Application Aggregate       | `RiskManagement.Domain/Aggregates/ApplicationAggregate/Application.cs` |
| ApplicationStatus           | `RiskManagement.Domain/ValueObjects/ApplicationStatus.cs`             |
| ApplicationInquiry          | `RiskManagement.Domain/Aggregates/ApplicationAggregate/ApplicationInquiry.cs` |
| Repository Interface        | `RiskManagement.Domain/Aggregates/ApplicationAggregate/IApplicationRepository.cs` |
| Domain Events               | `RiskManagement.Domain/Events/Application*.cs`, `InquiryCreatedEvent.cs` |
| Commands                    | `RiskManagement.Application/Commands/*.cs`                            |
| Queries                     | `RiskManagement.Application/Queries/*.cs`                             |
| Saga State + Events         | `RiskManagement.Application/Sagas/ApplicationCreation/`               |
| Saga State Machine          | `RiskManagement.Infrastructure/Sagas/ApplicationCreationStateMachine.cs` |
| Saga Consumers              | `RiskManagement.Infrastructure/Sagas/Consumers/`                      |
| Integration Event Consumers | `RiskManagement.Infrastructure/Consumers/`                            |
| API Controllers             | `RiskManagement.Api/Controllers/Applications*.cs`, `ProcessorController.cs`, `InquiryController.cs` |
| DI / Messaging Config       | `RiskManagement.Infrastructure/DependencyInjection.cs`                |
