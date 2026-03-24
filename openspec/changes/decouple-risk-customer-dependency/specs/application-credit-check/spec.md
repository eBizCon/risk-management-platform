## MODIFIED Requirements

### Requirement: Credit check during application creation
The credit check SHALL be performed exclusively within the saga pipeline via the `PerformCreditCheckConsumer`, not synchronously in command handlers. The `CreateApplicationHandler` and `CreateAndSubmitApplicationHandler` already follow this pattern. The `UpdateApplicationHandler`, `UpdateAndSubmitApplicationHandler`, and `SubmitApplicationHandler` SHALL also use the saga pipeline for credit checks instead of calling `ICreditCheckService` directly. No command handler SHALL have a direct dependency on `ICreditCheckService`.

#### Scenario: Application created with automatic credit check via saga
- **WHEN** an applicant creates an application for customer with ID 1
- **THEN** the saga SHALL fetch the customer profile including name, dateOfBirth, and address
- **THEN** the saga SHALL perform a credit check using that data via `PerformCreditCheckConsumer`
- **THEN** the application SHALL store the credit check result as its CreditReport
- **THEN** the scoring SHALL use the CreditReport values

#### Scenario: Application creation fails if customer not found
- **WHEN** an applicant creates an application for a non-existent customer
- **THEN** the system SHALL return a failure result with message "Kunde nicht gefunden"

#### Scenario: Application update performs fresh credit check via saga
- **WHEN** an applicant updates an existing application
- **THEN** the saga SHALL fetch the customer's current profile
- **THEN** the saga SHALL perform a fresh credit check
- **THEN** the application SHALL be updated with the fresh CreditReport

#### Scenario: Application submit performs fresh credit check via saga
- **WHEN** an applicant submits an existing application
- **THEN** the saga SHALL fetch the customer's current profile
- **THEN** the saga SHALL perform a fresh credit check
- **THEN** the application SHALL be submitted with the fresh CreditReport and scoring
