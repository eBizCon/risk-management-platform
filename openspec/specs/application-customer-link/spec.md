## MODIFIED Requirements

### Requirement: Customer validation on application creation
The Application Service SHALL validate that the referenced customer exists, has status `Active`, and has a non-null `CreditReport` before creating or updating an application. If the customer has no CreditReport, the system SHALL reject the application with a clear error.

#### Scenario: Create application with valid active customer with credit report
- **WHEN** an applicant creates an application with a `customerId` referencing an active customer who has a CreditReport
- **THEN** the system SHALL create the application successfully

#### Scenario: Create application with archived customer
- **WHEN** an applicant creates an application with a `customerId` referencing an archived customer
- **THEN** the system SHALL return HTTP 400 with message "Der ausgewählte Kunde ist archiviert"

#### Scenario: Create application with non-existent customer
- **WHEN** an applicant creates an application with a `customerId` referencing a non-existent customer
- **THEN** the system SHALL return HTTP 400 with message "Kunde nicht gefunden"

#### Scenario: Create application for customer without credit report
- **WHEN** an applicant creates an application with a `customerId` referencing an active customer who has no CreditReport
- **THEN** the system SHALL return HTTP 400 with message "Bitte führen Sie zuerst eine Bonitätsprüfung für den Kunden durch"

#### Scenario: Customer Service unavailable
- **WHEN** an applicant creates an application but the Customer Service is unreachable
- **THEN** the system SHALL return HTTP 503 with message "Kundendienst nicht erreichbar"

### Requirement: Application DTOs include customer information
The `ApplicationCreateDto` and `ApplicationUpdateDto` SHALL include `customerId` (int) but SHALL NOT include `employmentStatus` or `hasPaymentDefault`. These values are read from the Customer at scoring time. The `ApplicationResponse` SHALL include `customerId`, `customerName`, and the scoring snapshot fields.

#### Scenario: ApplicationCreateDto without employment and payment fields
- **WHEN** an applicant submits a new application
- **THEN** the request body SHALL contain `customerId`, `income`, `fixedCosts`, `desiredRate` — but NOT `employmentStatus` or `hasPaymentDefault`

#### Scenario: ApplicationResponse includes scoring snapshot
- **WHEN** an application is queried
- **THEN** the response SHALL include `customerId`, `customerName`, `scoringEmploymentStatus`, `scoringHasPaymentDefault`, and `scoringCreditScore`

## ADDED Requirements

### Requirement: Scoring reads customer profile data
When an application is created, submitted, or rescored, the system SHALL fetch the customer's current `EmploymentStatus` and `CreditReport` from the Customer Service (via internal API) and use these values for scoring. The Application SHALL NOT store `EmploymentStatus` or `HasPaymentDefault` as editable fields.

#### Scenario: Scoring uses customer employment status
- **WHEN** an application is created for a customer with `EmploymentStatus = "self_employed"`
- **THEN** the scoring SHALL use `SelfEmployed` employment status (10 point penalty with default config)

#### Scenario: Scoring uses customer payment default
- **WHEN** an application is created for a customer with `CreditReport.HasPaymentDefault = true`
- **THEN** the scoring SHALL apply the payment default penalty (25 points with default config)

#### Scenario: Rescoring reads fresh customer data
- **WHEN** a Risk Manager triggers rescoring for open applications
- **THEN** each application SHALL be rescored using the customer's CURRENT EmploymentStatus and CreditReport (not the snapshot)

### Requirement: Scoring snapshot on Application
The Application aggregate SHALL store denormalized scoring input snapshot fields: `ScoringEmploymentStatus` (string), `ScoringHasPaymentDefault` (bool), `ScoringCreditScore` (int?). These fields SHALL be set during scoring and SHALL be read-only (no setter exposed for external callers). They document which customer data was used for the score calculation.

#### Scenario: Snapshot stored on application creation
- **WHEN** an application is created for a customer with `EmploymentStatus = "employed"` and `CreditReport = { hasPaymentDefault: false, creditScore: 420 }`
- **THEN** the Application's snapshot fields SHALL be `ScoringEmploymentStatus = "employed"`, `ScoringHasPaymentDefault = false`, `ScoringCreditScore = 420`

#### Scenario: Snapshot updated on rescoring
- **WHEN** an application is rescored and the customer's employment status has changed from "employed" to "unemployed"
- **THEN** the Application's `ScoringEmploymentStatus` SHALL be updated to "unemployed"

#### Scenario: Snapshot fields in database
- **WHEN** an Application is persisted
- **THEN** the `applications` table SHALL contain columns `scoring_employment_status` (varchar), `scoring_has_payment_default` (bool), `scoring_credit_score` (int?)

### Requirement: Remove EmploymentStatus and HasPaymentDefault from Application
The Application aggregate SHALL remove the `EmploymentStatus` property and the `HasPaymentDefault` property. The `Application.Create()` and `UpdateDetails()` methods SHALL no longer accept these as parameters. The `ApplicationValidator` SHALL no longer validate `employmentStatus` or `hasPaymentDefault` fields.

#### Scenario: Application.Create no longer accepts employment status
- **WHEN** `Application.Create(...)` is called
- **THEN** it SHALL NOT have `employmentStatus` or `hasPaymentDefault` parameters
- **THEN** it SHALL receive the customer's data (employment status, credit report) to populate snapshot fields

#### Scenario: ApplicationValidator updated
- **WHEN** an application creation request is validated
- **THEN** the validator SHALL NOT check for `employmentStatus` or `hasPaymentDefault` fields

### Requirement: Application form without customer profile fields
The Application form SHALL NOT display employment status or payment default fields. These are managed on the Customer profile. The form SHALL only contain: Customer selection (dropdown), Income, Fixed Costs, and Desired Rate.

#### Scenario: Application form fields
- **WHEN** an applicant opens the application form (new or edit)
- **THEN** the form SHALL display: Customer dropdown, Income, Fixed Costs, Desired Rate
- **THEN** the form SHALL NOT display Employment Status or Payment Default fields

#### Scenario: Application detail shows scoring snapshot
- **WHEN** an applicant or processor views an application detail page
- **THEN** the page SHALL display the scoring snapshot values (employment status, payment default, credit score) as read-only information alongside the score and traffic light
