## ADDED Requirements

### Requirement: ICreditCheckService domain service in RiskManagement
The RiskManagement domain SHALL define an `ICreditCheckService` interface that accepts customer identification data (firstName, lastName, dateOfBirth, address) and returns a `CreditCheckResult` containing `HasPaymentDefault` (bool), `CreditScore` (int?), `CheckedAt` (DateTime), and `Provider` (string).

#### Scenario: Credit check returns result for valid customer data
- **WHEN** `ICreditCheckService.CheckAsync` is called with valid customer data (firstName="Max", lastName="Mustermann", dateOfBirth=1990-01-01, address with street/city/zip/country)
- **THEN** the service SHALL return a `CreditCheckResult` with `HasPaymentDefault`, `CreditScore`, `CheckedAt`, and `Provider` populated

### Requirement: MockSchufaProvider implements ICreditCheckService in RiskManagement
The `MockSchufaProvider` SHALL be implemented in `RiskManagement.Infrastructure.ExternalServices` implementing `ICreditCheckService`. The mock logic SHALL remain identical to the current CustomerManagement implementation: lastName containing "Verzug" or "Default" (case-insensitive) returns `HasPaymentDefault=true` and `CreditScore=250`; age over 65 returns `CreditScore=520`; age under 25 returns `CreditScore=350`; default case returns `HasPaymentDefault=false` and `CreditScore=420`. Provider SHALL be "schufa_mock".

#### Scenario: Default case returns score 420
- **WHEN** credit check is performed for customer "Max Mustermann" born 1990-01-01
- **THEN** the result SHALL have `HasPaymentDefault=false`, `CreditScore=420`, `Provider="schufa_mock"`

#### Scenario: LastName containing Verzug returns payment default
- **WHEN** credit check is performed for customer with lastName "Verzug"
- **THEN** the result SHALL have `HasPaymentDefault=true` and `CreditScore=250`

#### Scenario: Age over 65 returns higher score
- **WHEN** credit check is performed for customer older than 65
- **THEN** the result SHALL have `CreditScore=520` and `HasPaymentDefault=false`

#### Scenario: Age under 25 returns lower score
- **WHEN** credit check is performed for customer younger than 25
- **THEN** the result SHALL have `CreditScore=350` and `HasPaymentDefault=false`

### Requirement: CreditReport value object on Application aggregate
The `Application` aggregate in RiskManagement SHALL have a `CreditReport` property as a value object containing `HasPaymentDefault` (bool), `CreditScore` (int?), `CheckedAt` (DateTime), and `Provider` (string). This value object SHALL be set during application creation and SHALL be immutable after creation. The existing `HasPaymentDefault` and `CreditScore` flat fields on `Application` SHALL be replaced by this value object.

#### Scenario: Application stores credit report from check
- **WHEN** an application is created with a credit check result of `HasPaymentDefault=false`, `CreditScore=420`, `Provider="schufa_mock"`
- **THEN** the `Application.CreditReport` SHALL contain those exact values
- **THEN** `Application.CreditReport.CheckedAt` SHALL be set to the time of the check

#### Scenario: CreditReport is persisted with the application
- **WHEN** an application with a CreditReport is saved and reloaded from the database
- **THEN** all CreditReport fields SHALL be correctly restored

### Requirement: Credit check during application creation
The `Application.Create` factory method SHALL accept an `ICreditCheckService` instead of pre-fetched `hasPaymentDefault` and `creditScore` parameters. The command handlers (`CreateApplicationCommand`, `CreateAndSubmitApplicationCommand`) SHALL fetch customer master data via `ICustomerProfileService` (which now includes `DateOfBirth` and `Address`), call `ICreditCheckService.CheckAsync`, and pass the result to `Application.Create`. The `CreditReport` SHALL be stored on the application.

#### Scenario: Application created with automatic credit check
- **WHEN** an applicant creates an application for customer with ID 1
- **THEN** the system SHALL fetch the customer profile including name, dateOfBirth, and address
- **THEN** the system SHALL perform a credit check using that data
- **THEN** the application SHALL store the credit check result as its CreditReport
- **THEN** the scoring SHALL use the CreditReport values

#### Scenario: Application creation fails if customer not found
- **WHEN** an applicant creates an application for a non-existent customer
- **THEN** the system SHALL return a failure result with message "Kunde nicht gefunden"

### Requirement: Extended customer profile includes address and date of birth
The `ICustomerProfileService.CustomerProfile` record SHALL include `DateOfBirth` (string, ISO format) and `Address` (record with Street, City, ZipCode, Country). The `CreditReport` field SHALL be removed from `CustomerProfile`. The `GET /api/internal/customers/{id}` endpoint SHALL return `DateOfBirth` and `Address` fields and SHALL no longer return `CreditReport`.

#### Scenario: Customer profile includes address and date of birth
- **WHEN** RiskManagement fetches customer profile for an existing customer
- **THEN** the response SHALL include `DateOfBirth`, `Address.Street`, `Address.City`, `Address.ZipCode`, `Address.Country`
- **THEN** the response SHALL NOT include `CreditReport`

### Requirement: Remove credit report from CustomerManagement
The CustomerManagement bounded context SHALL remove: `ICreditReportProvider` domain service interface, `MockSchufaProvider` infrastructure implementation, `CreditReport` value object, `CreditReportReceivedEvent` domain event, `RequestCreditReportCommand` and its handler, the `CreditReport` property from the `Customer` aggregate, the `UpdateCreditReport` method from the `Customer` aggregate, and the `POST /api/customers/{id}/credit-report` API endpoint. A database migration SHALL remove the credit report columns from the Customer table.

#### Scenario: Customer no longer has CreditReport property
- **WHEN** a customer is created or updated
- **THEN** no CreditReport data SHALL be stored on the Customer entity

#### Scenario: Credit report API endpoint no longer exists
- **WHEN** a client calls `POST /api/customers/{id}/credit-report`
- **THEN** the system SHALL return HTTP 404

#### Scenario: Internal customer API excludes credit report
- **WHEN** `GET /api/internal/customers/{id}` is called
- **THEN** the response SHALL NOT contain a `CreditReport` field
- **THEN** the response SHALL contain `DateOfBirth` and `Address` fields

### Requirement: Frontend removes credit report prerequisite
The frontend SHALL no longer require a credit report to exist before creating an application. The "Bonitätsprüfung anfordern" button/action on the customer detail page SHALL be removed. The application creation flow SHALL work without a prior credit check since the check happens automatically during creation.

#### Scenario: Application can be created without prior credit check
- **WHEN** an applicant navigates to create an application for a customer
- **THEN** the system SHALL NOT check for an existing credit report
- **THEN** the application SHALL be created with an automatic credit check

#### Scenario: Customer detail page has no credit report action
- **WHEN** an applicant views a customer detail page
- **THEN** there SHALL be no "Bonitätsprüfung anfordern" button or action
