## ADDED Requirements

### Requirement: ApplicationCreationSaga State Machine
The system SHALL implement a MassTransit `MassTransitStateMachine<ApplicationCreationState>` that orchestrates the asynchronous application creation process. The saga SHALL define the following states: `FetchingCustomer`, `CheckingCredit`, `Finalizing`, `Completed`, `Failed`. The saga SHALL transition through states sequentially: Initial → FetchingCustomer → CheckingCredit → Finalizing → Completed. Any step MAY transition to `Failed` on error.

#### Scenario: Saga starts on ApplicationCreationStarted event
- **WHEN** an `ApplicationCreationStarted` event is published with a CorrelationId, ApplicationId, CustomerId, and application input data
- **THEN** the saga SHALL create a new saga instance, store the input data on the saga state, publish a `FetchCustomerProfile` message, and transition to `FetchingCustomer`

#### Scenario: Saga transitions from FetchingCustomer to CheckingCredit
- **WHEN** the saga is in `FetchingCustomer` state and receives a `CustomerProfileFetched` event with customer data (FirstName, LastName, EmploymentStatus, DateOfBirth, Address)
- **THEN** the saga SHALL store the customer data on the saga state, publish a `PerformCreditCheck` message, and transition to `CheckingCredit`

#### Scenario: Saga transitions from CheckingCredit to Finalizing
- **WHEN** the saga is in `CheckingCredit` state and receives a `CreditCheckCompleted` event with HasPaymentDefault, CreditScore, CheckedAt, Provider
- **THEN** the saga SHALL store the credit check data on the saga state, publish a `FinalizeApplication` message with all accumulated data, and transition to `Finalizing`

#### Scenario: Saga completes on ApplicationCreationCompleted
- **WHEN** the saga is in `Finalizing` state and receives an `ApplicationCreationCompleted` event
- **THEN** the saga SHALL set `CompletedAt` and transition to `Completed` and finalize (mark for cleanup)

#### Scenario: Saga transitions to Failed on error in any state
- **WHEN** the saga receives an `ApplicationCreationFailed` event in any active state (FetchingCustomer, CheckingCredit, Finalizing)
- **THEN** the saga SHALL store the failure reason and transition to `Failed` and finalize

### Requirement: ApplicationCreationState saga instance
The system SHALL define an `ApplicationCreationState` class implementing `SagaStateMachineInstance` with: `CorrelationId` (Guid, primary key), `CurrentState` (string), input data fields (ApplicationId, CustomerId, Income, FixedCosts, DesiredRate, UserEmail), intermediate result fields (FirstName, LastName, EmploymentStatus, DateOfBirth, Street, City, ZipCode, Country, HasPaymentDefault, CreditScore, CreditCheckedAt, CreditProvider), and metadata fields (FailureReason, CreatedAt, CompletedAt).

#### Scenario: Saga state persisted in PostgreSQL
- **WHEN** a saga instance is created or updated
- **THEN** the state SHALL be persisted in a `saga_application_creation_state` table in the RiskManagement database via EF Core

#### Scenario: Saga state uses CorrelationId as primary key
- **WHEN** a saga instance is created
- **THEN** the `CorrelationId` SHALL be the primary key with a unique constraint

### Requirement: Saga message contracts
The system SHALL define the following message records for saga communication. All messages SHALL include a `CorrelationId` (Guid) for saga correlation.

- `ApplicationCreationStarted`: CorrelationId, ApplicationId (int), CustomerId (int), Income (double), FixedCosts (double), DesiredRate (double), UserEmail (string)
- `FetchCustomerProfile`: CorrelationId, CustomerId (int)
- `CustomerProfileFetched`: CorrelationId, FirstName, LastName, EmploymentStatus, DateOfBirth, Street, City, ZipCode, Country
- `PerformCreditCheck`: CorrelationId, FirstName, LastName, DateOfBirth (DateOnly), Street, City, ZipCode, Country
- `CreditCheckCompleted`: CorrelationId, HasPaymentDefault (bool), CreditScore (int?), CheckedAt (DateTime), Provider (string)
- `FinalizeApplication`: CorrelationId, ApplicationId (int), CustomerId (int), Income (double), FixedCosts (double), DesiredRate (double), UserEmail (string), EmploymentStatus (string), HasPaymentDefault (bool), CreditScore (int?), CreditCheckedAt (DateTime), CreditProvider (string)
- `ApplicationCreationCompleted`: CorrelationId
- `ApplicationCreationFailed`: CorrelationId, Reason (string)

#### Scenario: All messages correlate by CorrelationId
- **WHEN** any saga message is published
- **THEN** it SHALL contain the same CorrelationId that was set on the `ApplicationCreationStarted` event

#### Scenario: FinalizeApplication carries all accumulated data
- **WHEN** a `FinalizeApplication` message is published
- **THEN** it SHALL contain all data needed to finalize the application without reading saga state (customer profile, credit check result, application input)

### Requirement: FetchCustomerProfileConsumer
The system SHALL implement a MassTransit `IConsumer<FetchCustomerProfile>` that fetches customer data via `ICustomerProfileService.GetCustomerProfileAsync()`. On success, the consumer SHALL publish `CustomerProfileFetched`. On failure (customer not found), the consumer SHALL publish `ApplicationCreationFailed` with reason "Kunde nicht gefunden".

#### Scenario: Customer profile fetched successfully
- **WHEN** `FetchCustomerProfileConsumer` receives a `FetchCustomerProfile` message for an existing customer
- **THEN** the consumer SHALL call `ICustomerProfileService.GetCustomerProfileAsync()` and publish `CustomerProfileFetched` with the customer data

#### Scenario: Customer not found
- **WHEN** `FetchCustomerProfileConsumer` receives a `FetchCustomerProfile` message for a non-existent customer
- **THEN** the consumer SHALL publish `ApplicationCreationFailed` with reason "Kunde nicht gefunden"

#### Scenario: HTTP failure triggers retry
- **WHEN** `ICustomerProfileService.GetCustomerProfileAsync()` throws an exception (e.g., HTTP timeout)
- **THEN** MassTransit's retry policy SHALL retry the consumer automatically

### Requirement: PerformCreditCheckConsumer
The system SHALL implement a MassTransit `IConsumer<PerformCreditCheck>` that performs a credit check via `ICreditCheckService.CheckAsync()`. On success, the consumer SHALL publish `CreditCheckCompleted`.

#### Scenario: Credit check performed successfully
- **WHEN** `PerformCreditCheckConsumer` receives a `PerformCreditCheck` message
- **THEN** the consumer SHALL call `ICreditCheckService.CheckAsync()` with the provided customer data and publish `CreditCheckCompleted`

#### Scenario: Credit check failure triggers retry
- **WHEN** `ICreditCheckService.CheckAsync()` throws an exception
- **THEN** MassTransit's retry policy SHALL retry the consumer automatically

### Requirement: FinalizeApplicationConsumer
The system SHALL implement a MassTransit `IConsumer<FinalizeApplication>` that finalizes the application: loads the Application aggregate by ID, calls `Finalize()` with the accumulated data (EmploymentStatus, CreditReport, ScoringConfig), saves, and publishes `ApplicationCreationCompleted`. On failure, the consumer SHALL publish `ApplicationCreationFailed`.

#### Scenario: Application finalized with scoring
- **WHEN** `FinalizeApplicationConsumer` receives a `FinalizeApplication` message
- **THEN** the consumer SHALL load the Application aggregate, call `Finalize()` with EmploymentStatus, CreditReport, IScoringService, and ScoringConfig
- **THEN** the Application status SHALL transition from `Processing` to `Draft`
- **THEN** the consumer SHALL save and publish `ApplicationCreationCompleted`

#### Scenario: Application finalized and auto-submitted
- **WHEN** `FinalizeApplicationConsumer` receives a `FinalizeApplication` message with `AutoSubmit = true`
- **THEN** the consumer SHALL call `Finalize()` followed by `Submit()` on the Application aggregate
- **THEN** the Application status SHALL transition from `Processing` to `Submitted`

#### Scenario: Application not found during finalization
- **WHEN** `FinalizeApplicationConsumer` receives a `FinalizeApplication` message but the Application does not exist in the database
- **THEN** the consumer SHALL publish `ApplicationCreationFailed` with reason "Antrag nicht gefunden"

### Requirement: Application aggregate Processing status
The `Application` aggregate SHALL support a `Processing` status representing an application whose creation saga is in progress. The aggregate SHALL provide a `CreateProcessing()` factory method that creates a minimal application with only `CustomerId`, `CreatedBy`, input values (Income, FixedCosts, DesiredRate), and `Status = Processing`.

#### Scenario: Create application in Processing state
- **WHEN** `Application.CreateProcessing()` is called with customerId, income, fixedCosts, desiredRate, and createdBy
- **THEN** the application SHALL have `Status = Processing`, `CreditReport = null`, `EmploymentStatus = null`, `Score = null`

#### Scenario: Processing application cannot be submitted
- **WHEN** `Submit()` is called on an application with `Status = Processing`
- **THEN** the system SHALL throw an `InvalidStatusTransitionException`

#### Scenario: Processing application cannot be approved or rejected
- **WHEN** `Approve()` or `Reject()` is called on an application with `Status = Processing`
- **THEN** the system SHALL throw an `InvalidStatusTransitionException`

### Requirement: Application aggregate Finalize method
The `Application` aggregate SHALL provide a `Finalize()` method that transitions from `Processing` to `Draft`. The method SHALL accept EmploymentStatus, CreditReport, IScoringService, ScoringConfig, and ScoringConfigVersionId. It SHALL set EmploymentStatus, CreditReport, apply scoring, and transition to `Draft`.

#### Scenario: Finalize sets all domain data and transitions to Draft
- **WHEN** `Finalize()` is called on an application with `Status = Processing`
- **THEN** the application SHALL have EmploymentStatus, CreditReport, Score, TrafficLight, and ScoringReasons set
- **THEN** the status SHALL be `Draft`

#### Scenario: Finalize on non-Processing application throws
- **WHEN** `Finalize()` is called on an application with `Status != Processing`
- **THEN** the system SHALL throw a `DomainException`

### Requirement: Application aggregate Failed status
The `Application` aggregate SHALL support a `Failed` status representing a creation saga that failed permanently. The aggregate SHALL provide a `MarkFailed(string reason)` method that transitions from `Processing` to `Failed` and stores the failure reason.

#### Scenario: Mark application as failed
- **WHEN** `MarkFailed("Kunde nicht gefunden")` is called on an application with `Status = Processing`
- **THEN** the application SHALL have `Status = Failed` and the failure reason SHALL be stored

#### Scenario: Failed application can be deleted
- **WHEN** `Delete()` is called on an application with `Status = Failed`
- **THEN** the application SHALL be deleted (same as Draft deletion)

### Requirement: CreateApplicationCommand returns 202 Accepted
The `CreateApplicationHandler` SHALL create a `Processing` application, save it, publish an `ApplicationCreationStarted` event to the message bus, and return `202 Accepted` with `{ id, status: "processing" }`. The handler SHALL NOT perform the customer profile fetch, credit check, or scoring synchronously.

#### Scenario: Application created as Processing
- **WHEN** an applicant sends `POST /api/applications` with valid application data
- **THEN** the system SHALL return HTTP 202 with the application ID and status "processing"
- **THEN** the application SHALL exist in the database with `Status = Processing`

#### Scenario: Validation still happens synchronously
- **WHEN** an applicant sends `POST /api/applications` with invalid data (e.g., negative income)
- **THEN** the system SHALL return HTTP 400 with validation errors immediately (no saga started)

### Requirement: CreateAndSubmitApplicationCommand returns 202 Accepted
The `CreateAndSubmitApplicationHandler` SHALL follow the same async pattern as `CreateApplicationHandler` but include an `AutoSubmit = true` flag in the saga start event so that the `FinalizeApplicationConsumer` submits the application after finalization.

#### Scenario: Application created and auto-submitted asynchronously
- **WHEN** an applicant sends `POST /api/applications?submit=true` with valid application data
- **THEN** the system SHALL return HTTP 202 with the application ID and status "processing"
- **THEN** after saga completion, the application SHALL have `Status = Submitted`

### Requirement: Frontend polls for application completion
The frontend SHALL poll `GET /api/applications/{id}` after receiving a `202 Accepted` response until the application status is no longer `processing`. The poll interval SHALL be 2 seconds. The frontend SHALL display a loading indicator during processing and show the completed application or an error message on completion/failure.

#### Scenario: Frontend displays processing state
- **WHEN** the frontend receives `202 Accepted` with status "processing"
- **THEN** the frontend SHALL display a loading indicator with text "Antrag wird verarbeitet..."
- **THEN** the frontend SHALL poll `GET /api/applications/{id}` every 2 seconds

#### Scenario: Frontend displays completed application
- **WHEN** a poll response returns status "draft" or "submitted"
- **THEN** the frontend SHALL stop polling and display the completed application details

#### Scenario: Frontend displays failure
- **WHEN** a poll response returns status "failed"
- **THEN** the frontend SHALL stop polling and display an error message with the failure reason
- **THEN** the frontend SHALL offer a "Erneut versuchen" (retry) button and a "Löschen" (delete) button
