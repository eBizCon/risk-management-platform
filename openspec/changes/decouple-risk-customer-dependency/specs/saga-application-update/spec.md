## ADDED Requirements

### Requirement: ApplicationUpdateStarted saga event
The system SHALL define an `ApplicationUpdateStarted` message record with: `CorrelationId` (Guid), `ApplicationId` (int), `CustomerId` (int), `Income` (double), `FixedCosts` (double), `DesiredRate` (double), `UserEmail` (string), and `AutoSubmit` (bool). This event SHALL trigger the saga pipeline for application update and submit flows.

#### Scenario: ApplicationUpdateStarted published for Update flow
- **WHEN** a user updates an application (without submit)
- **THEN** the handler SHALL publish `ApplicationUpdateStarted` with `AutoSubmit=false` and the DTO values for Income, FixedCosts, DesiredRate

#### Scenario: ApplicationUpdateStarted published for UpdateAndSubmit flow
- **WHEN** a user updates and submits an application
- **THEN** the handler SHALL publish `ApplicationUpdateStarted` with `AutoSubmit=true` and the DTO values for Income, FixedCosts, DesiredRate

#### Scenario: ApplicationUpdateStarted published for Submit-only flow
- **WHEN** a user submits an existing application without changes
- **THEN** the handler SHALL publish `ApplicationUpdateStarted` with `AutoSubmit=true` and the application's current values for Income, FixedCosts, DesiredRate

### Requirement: OperationType on saga state
The `ApplicationCreationState` SHALL include an `OperationType` string property. The value SHALL be `"Create"` for flows triggered by `ApplicationCreationStarted` and `"Update"` for flows triggered by `ApplicationUpdateStarted`. This property SHALL be persisted in the database and used by the state machine to determine the finalize step.

#### Scenario: OperationType set on creation flow
- **WHEN** `ApplicationCreationStarted` triggers the saga
- **THEN** `ApplicationCreationState.OperationType` SHALL be `"Create"`

#### Scenario: OperationType set on update flow
- **WHEN** `ApplicationUpdateStarted` triggers the saga
- **THEN** `ApplicationCreationState.OperationType` SHALL be `"Update"`

### Requirement: State machine handles ApplicationUpdateStarted
The `ApplicationCreationStateMachine` SHALL have an additional `Initially` block for `ApplicationUpdateStarted`. When this event is received, the state machine SHALL set `OperationType` to `"Update"`, store all event data in the saga state, and transition to `FetchingCustomer` (triggering the same pipeline as the Create flow).

#### Scenario: Update flow enters FetchingCustomer state
- **WHEN** `ApplicationUpdateStarted` is published
- **THEN** the saga SHALL transition to `FetchingCustomer`
- **THEN** the saga SHALL publish `FetchCustomerProfile` with the CustomerId from the event

#### Scenario: Update flow reuses existing pipeline steps
- **WHEN** the saga is in `FetchingCustomer` state after an update event
- **THEN** `CustomerProfileFetched` SHALL transition to `CheckingCredit` (same as Create flow)
- **THEN** `CreditCheckCompleted` SHALL transition to `Finalizing`

### Requirement: Conditional finalize event based on OperationType
When the saga is in `CheckingCredit` state and receives `CreditCheckCompleted`, the state machine SHALL publish `FinalizeApplication` (existing) if `OperationType` is `"Create"`, or `FinalizeApplicationUpdate` (new) if `OperationType` is `"Update"`.

#### Scenario: Create flow publishes FinalizeApplication
- **WHEN** `CreditCheckCompleted` is received and `OperationType` is `"Create"`
- **THEN** the saga SHALL publish `FinalizeApplication` with all collected data

#### Scenario: Update flow publishes FinalizeApplicationUpdate
- **WHEN** `CreditCheckCompleted` is received and `OperationType` is `"Update"`
- **THEN** the saga SHALL publish `FinalizeApplicationUpdate` with all collected data including Income, FixedCosts, DesiredRate, and AutoSubmit

### Requirement: FinalizeApplicationUpdate message
The system SHALL define a `FinalizeApplicationUpdate` message record with: `CorrelationId` (Guid), `ApplicationId` (int), `CustomerId` (int), `Income` (double), `FixedCosts` (double), `DesiredRate` (double), `UserEmail` (string), `EmploymentStatus` (string), `HasPaymentDefault` (bool), `CreditScore` (int?), `CreditCheckedAt` (DateTime), `CreditProvider` (string), and `AutoSubmit` (bool).

#### Scenario: FinalizeApplicationUpdate contains all required data
- **WHEN** the saga publishes `FinalizeApplicationUpdate`
- **THEN** the message SHALL contain the original financial data from the handler AND the fresh customer/credit data from the saga pipeline

### Requirement: FinalizeApplicationUpdateConsumer
The system SHALL implement a `FinalizeApplicationUpdateConsumer` that consumes `FinalizeApplicationUpdate` messages. It SHALL load the application by ID, load the current scoring configuration, call `application.UpdateDetails()` with the message data (using fresh EmploymentStatus and CreditReport from the saga), and if `AutoSubmit` is true, call `application.Submit()`. After saving, it SHALL publish `ApplicationCreationCompleted`.

#### Scenario: Update without submit
- **WHEN** `FinalizeApplicationUpdate` is consumed with `AutoSubmit=false`
- **THEN** the consumer SHALL call `UpdateDetails()` with fresh external data
- **THEN** the consumer SHALL NOT call `Submit()`
- **THEN** the consumer SHALL save and publish `ApplicationCreationCompleted`

#### Scenario: Update with submit
- **WHEN** `FinalizeApplicationUpdate` is consumed with `AutoSubmit=true`
- **THEN** the consumer SHALL call `UpdateDetails()` with fresh external data
- **THEN** the consumer SHALL call `Submit()`
- **THEN** the consumer SHALL save and publish `ApplicationCreationCompleted`

#### Scenario: Application not found during finalize
- **WHEN** `FinalizeApplicationUpdate` is consumed and the application does not exist
- **THEN** the consumer SHALL publish `ApplicationCreationFailed` with reason "Antrag nicht gefunden"

#### Scenario: Error during finalize
- **WHEN** an exception occurs during `FinalizeApplicationUpdate` processing
- **THEN** the consumer SHALL call `application.MarkFailed()` with the error message
- **THEN** the consumer SHALL publish `ApplicationCreationFailed`

### Requirement: UpdateApplicationHandler publishes saga event
The `UpdateApplicationHandler` SHALL no longer depend on `ICustomerProfileService` or `ICreditCheckService`. It SHALL validate ownership, validate the DTO, set the application to Processing status, save, and publish `ApplicationUpdateStarted` with `AutoSubmit=false`. It SHALL return the application in Processing state.

#### Scenario: Update handler triggers saga
- **WHEN** a user updates an application with valid data
- **THEN** the handler SHALL set application status to Processing
- **THEN** the handler SHALL publish `ApplicationUpdateStarted`
- **THEN** the handler SHALL return the application with Processing status

#### Scenario: Update handler validation failure
- **WHEN** a user updates an application with invalid data
- **THEN** the handler SHALL return a validation failure WITHOUT publishing any saga event

### Requirement: UpdateAndSubmitApplicationHandler publishes saga event
The `UpdateAndSubmitApplicationHandler` SHALL no longer depend on `ICustomerProfileService` or `ICreditCheckService`. It SHALL validate ownership, validate the DTO, set the application to Processing status, save, and publish `ApplicationUpdateStarted` with `AutoSubmit=true`. It SHALL return the application in Processing state.

#### Scenario: UpdateAndSubmit handler triggers saga
- **WHEN** a user updates and submits an application with valid data
- **THEN** the handler SHALL set application status to Processing
- **THEN** the handler SHALL publish `ApplicationUpdateStarted`
- **THEN** the handler SHALL return the application with Processing status

### Requirement: SubmitApplicationHandler publishes saga event
The `SubmitApplicationHandler` SHALL no longer depend on `IScoringConfigRepository` or `IScoringService` directly. It SHALL validate ownership, set the application to Processing status, save, and publish `ApplicationUpdateStarted` with `AutoSubmit=true` and the application's current financial values (Income, FixedCosts, DesiredRate, CustomerId). It SHALL return the application in Processing state.

#### Scenario: Submit handler triggers saga with current values
- **WHEN** a user submits an existing draft application
- **THEN** the handler SHALL read the application's current Income, FixedCosts, DesiredRate, CustomerId
- **THEN** the handler SHALL publish `ApplicationUpdateStarted` with those values and `AutoSubmit=true`
- **THEN** the handler SHALL return the application with Processing status

#### Scenario: Submit handler rejects non-draft application
- **WHEN** a user submits an application that is not in Draft status
- **THEN** the handler SHALL return a failure result without publishing any saga event

### Requirement: Error handling for update saga
The existing `ApplicationCreationFailed` event and `DuringAny` error handling in the state machine SHALL apply to update flows as well. If any step fails (FetchCustomerProfile, CreditCheck, FinalizeUpdate), the saga SHALL transition to Failed state.

#### Scenario: Customer profile fetch fails during update
- **WHEN** `FetchCustomerProfile` fails for an update flow
- **THEN** the saga SHALL transition to Failed state with the error reason

#### Scenario: Credit check fails during update
- **WHEN** `PerformCreditCheck` fails for an update flow
- **THEN** the saga SHALL transition to Failed state with the error reason
