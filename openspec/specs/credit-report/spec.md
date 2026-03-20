## ADDED Requirements

### Requirement: CreditReport Value Object
The system SHALL represent credit check results as an immutable `CreditReport` Value Object in `CustomerManagement.Domain` with the following fields: `HasPaymentDefault` (bool, required), `CreditScore` (int?, optional external credit score in range 100-600), `CheckedAt` (DateTime, required), `Provider` (string, required, e.g. "schufa_mock").

#### Scenario: Valid CreditReport creation
- **WHEN** a CreditReport is created with `HasPaymentDefault = true`, `CreditScore = 250`, `CheckedAt = 2026-03-15T10:00:00Z`, `Provider = "schufa_mock"`
- **THEN** it SHALL be a valid Value Object with equality based on all fields

#### Scenario: CreditReport without external score
- **WHEN** a CreditReport is created with `CreditScore = null`
- **THEN** it SHALL be valid (the external score is optional)

#### Scenario: CreditReport with invalid score range
- **WHEN** a CreditReport is created with `CreditScore = 50` (below 100) or `CreditScore = 700` (above 600)
- **THEN** the system SHALL reject it with a DomainException

#### Scenario: CreditReport with empty provider
- **WHEN** a CreditReport is created with an empty or null `Provider`
- **THEN** the system SHALL reject it with a DomainException

### Requirement: CreditReport on Customer aggregate
The Customer aggregate SHALL have a nullable `CreditReport` property. A newly created customer SHALL have `CreditReport = null` (no check performed yet). The CreditReport SHALL be updated via an explicit `UpdateCreditReport(CreditReport)` method on the Customer aggregate.

#### Scenario: New customer has no CreditReport
- **WHEN** a customer is created
- **THEN** `CreditReport` SHALL be null

#### Scenario: CreditReport updated on customer
- **WHEN** `Customer.UpdateCreditReport(creditReport)` is called with a valid CreditReport
- **THEN** the Customer's `CreditReport` property SHALL be set to the new value
- **THEN** the Customer's `UpdatedAt` SHALL be set to the current UTC time

#### Scenario: CreditReport update on archived customer
- **WHEN** `Customer.UpdateCreditReport(creditReport)` is called on an archived customer
- **THEN** the system SHALL throw a DomainException "Kunde muss aktiv sein für diese Aktion"

### Requirement: ICreditReportProvider domain interface
The system SHALL define an `ICreditReportProvider` interface in `CustomerManagement.Domain` as a Port with the method `CheckAsync(string firstName, string lastName, DateOnly dateOfBirth, Address address) → Task<CreditReport>`.

#### Scenario: Provider returns CreditReport
- **WHEN** `ICreditReportProvider.CheckAsync(...)` is called with valid customer data
- **THEN** it SHALL return a `CreditReport` Value Object

### Requirement: MockSchufaProvider implementation
The system SHALL provide a `MockSchufaProvider` implementation of `ICreditReportProvider` in `CustomerManagement.Infrastructure`. The mock SHALL return deterministic results based on customer data: LastName containing "Verzug" or "Default" SHALL result in `HasPaymentDefault = true` with `CreditScore = 250`. Age > 65 SHALL result in `CreditScore = 520`. Age < 25 SHALL result in `CreditScore = 350`. Default case SHALL result in `HasPaymentDefault = false` with `CreditScore = 420`. The `Provider` field SHALL always be `"schufa_mock"`.

#### Scenario: Customer with payment default indicator in name
- **WHEN** a credit check is performed for a customer with LastName "Müller-Verzug"
- **THEN** the result SHALL have `HasPaymentDefault = true` and `CreditScore = 250`

#### Scenario: Customer with default indicator in name (English)
- **WHEN** a credit check is performed for a customer with LastName "Default"
- **THEN** the result SHALL have `HasPaymentDefault = true` and `CreditScore = 250`

#### Scenario: Senior customer
- **WHEN** a credit check is performed for a customer born before 65 years ago
- **THEN** the result SHALL have `HasPaymentDefault = false` and `CreditScore = 520`

#### Scenario: Young customer
- **WHEN** a credit check is performed for a customer born less than 25 years ago
- **THEN** the result SHALL have `HasPaymentDefault = false` and `CreditScore = 350`

#### Scenario: Standard customer
- **WHEN** a credit check is performed for a customer aged 25-65 without name indicators
- **THEN** the result SHALL have `HasPaymentDefault = false` and `CreditScore = 420`

#### Scenario: Provider field is always schufa_mock
- **WHEN** any credit check is performed via MockSchufaProvider
- **THEN** the `Provider` field SHALL be `"schufa_mock"` and `CheckedAt` SHALL be the current UTC time

### Requirement: Request credit check command
The system SHALL provide a `RequestCreditReportCommand(int CustomerId, string UserEmail)` that triggers a credit check for the specified customer. The handler SHALL load the customer, call `ICreditReportProvider.CheckAsync(...)` with the customer's data, call `Customer.UpdateCreditReport(creditReport)`, save, and publish a `CreditReportReceivedEvent`.

#### Scenario: Successful credit check
- **WHEN** a Sachbearbeiter requests a credit check for an active customer via `POST /api/customers/{id}/credit-check`
- **THEN** the system SHALL call the credit report provider with the customer's data
- **THEN** the system SHALL update the customer's CreditReport
- **THEN** the system SHALL return HTTP 200 with the updated customer data including the new CreditReport

#### Scenario: Credit check for non-existent customer
- **WHEN** a credit check is requested for a non-existent customer ID
- **THEN** the system SHALL return HTTP 404

#### Scenario: Credit check for archived customer
- **WHEN** a credit check is requested for an archived customer
- **THEN** the system SHALL return HTTP 400 with message "Kunde muss aktiv sein für diese Aktion"

#### Scenario: Credit check replaces previous report
- **WHEN** a credit check is requested for a customer who already has a CreditReport
- **THEN** the system SHALL replace the existing CreditReport with the new one

### Requirement: CreditReportReceivedEvent domain event
The system SHALL publish a `CreditReportReceivedEvent(CustomerId customerId, bool hasPaymentDefault, int? creditScore)` after a credit report is successfully stored on a customer.

#### Scenario: Event published after credit check
- **WHEN** a credit check is completed and stored on the customer
- **THEN** the system SHALL publish a `CreditReportReceivedEvent` with the customer ID and report summary

### Requirement: CreditReport in Customer API responses
The `CustomerResponse` DTO SHALL include an optional `creditReport` object with fields `hasPaymentDefault` (bool), `creditScore` (int?), `checkedAt` (string, ISO 8601), and `provider` (string). If the customer has no credit report, the field SHALL be null.

#### Scenario: Customer with credit report in response
- **WHEN** a customer with a CreditReport is queried via `GET /api/customers/{id}`
- **THEN** the response SHALL include the `creditReport` object with all fields

#### Scenario: Customer without credit report in response
- **WHEN** a customer without a CreditReport is queried
- **THEN** the `creditReport` field SHALL be null

### Requirement: CreditReport in internal Customer API
The internal endpoint `GET /api/internal/customers/{id}` SHALL include `creditReport` (nullable object with `hasPaymentDefault`, `creditScore`, `checkedAt`, `provider`) in the response for the Application Service to consume during scoring.

#### Scenario: Internal API includes credit report
- **WHEN** the Application Service calls `GET /api/internal/customers/{id}` for a customer with a CreditReport
- **THEN** the response SHALL include the `creditReport` object

#### Scenario: Internal API with null credit report
- **WHEN** the Application Service calls `GET /api/internal/customers/{id}` for a customer without a CreditReport
- **THEN** the `creditReport` field SHALL be null

### Requirement: Credit check UI on Customer detail page
The Customer detail page (`/customers/{id}`) SHALL display the current CreditReport (if present) and provide a "Bonität prüfen" button to trigger a new credit check.

#### Scenario: Customer with existing credit report
- **WHEN** a Sachbearbeiter views a customer with a CreditReport
- **THEN** the page SHALL display HasPaymentDefault (Ja/Nein), CreditScore, CheckedAt, and Provider
- **THEN** the page SHALL show a "Bonität prüfen" button to request a new check

#### Scenario: Customer without credit report
- **WHEN** a Sachbearbeiter views a customer without a CreditReport
- **THEN** the page SHALL display "Keine Bonitätsprüfung vorhanden"
- **THEN** the page SHALL show a "Bonität prüfen" button

#### Scenario: Trigger credit check from UI
- **WHEN** a Sachbearbeiter clicks "Bonität prüfen"
- **THEN** the system SHALL call `POST /api/customers/{id}/credit-check`
- **THEN** the page SHALL refresh and display the new CreditReport

### Requirement: EF Core mapping for CreditReport
The CreditReport Value Object SHALL be mapped as owned entity columns on the `customers` table: `credit_report_has_payment_default` (bool), `credit_report_credit_score` (int?), `credit_report_checked_at` (timestamp), `credit_report_provider` (varchar). All columns SHALL be nullable (null when no credit report exists).

#### Scenario: Customer with credit report persisted
- **WHEN** a customer with a CreditReport is saved
- **THEN** the `credit_report_*` columns SHALL contain the report values

#### Scenario: Customer without credit report persisted
- **WHEN** a customer without a CreditReport is saved
- **THEN** all `credit_report_*` columns SHALL be null
