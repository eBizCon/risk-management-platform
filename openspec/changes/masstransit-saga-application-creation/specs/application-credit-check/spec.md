## MODIFIED Requirements

### Requirement: Credit check during application creation
The command handlers (`CreateApplicationCommand`, `CreateAndSubmitApplicationCommand`) SHALL NOT perform the credit check synchronously. Instead, the credit check SHALL be performed asynchronously by the `PerformCreditCheckConsumer` as part of the `ApplicationCreationSaga`. The `ICreditCheckService` interface and `MockSchufaProvider` implementation SHALL remain unchanged. The `UpdateApplicationCommand` and `UpdateAndSubmitApplicationCommand` SHALL continue to perform the credit check synchronously (no change for update flows).

#### Scenario: Application created without synchronous credit check
- **WHEN** an applicant creates an application for customer with ID 1
- **THEN** the system SHALL NOT call `ICreditCheckService.CheckAsync()` synchronously
- **THEN** the system SHALL return HTTP 202 with status "processing"
- **THEN** the credit check SHALL be performed asynchronously by the saga's `PerformCreditCheckConsumer`

#### Scenario: Application creation fails if customer not found (async)
- **WHEN** the saga's `FetchCustomerProfileConsumer` cannot find the customer
- **THEN** the saga SHALL transition to `Failed` with reason "Kunde nicht gefunden"
- **THEN** the application status SHALL be updated to `Failed`

#### Scenario: Update flow still performs synchronous credit check
- **WHEN** an applicant updates an existing draft application
- **THEN** the system SHALL perform the credit check synchronously as before
- **THEN** the system SHALL return HTTP 200 with the updated application
