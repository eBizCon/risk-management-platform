## ADDED Requirements

### Requirement: Application references Customer by ID
The Application aggregate SHALL replace the `Name` (string) property with a `CustomerId` (strongly typed `CustomerId`) property. Applications SHALL reference customers by ID only — no navigation property or embedded customer data.

#### Scenario: Application stores CustomerId
- **WHEN** an Application is created
- **THEN** it SHALL have a `CustomerId` property of type `CustomerId` instead of a `Name` property

#### Scenario: Database column change
- **WHEN** the applications table is configured
- **THEN** the `name` column SHALL be replaced with a `customer_id` integer column

### Requirement: Customer validation on application creation
The Application Service SHALL validate that the referenced customer exists and has status `Active` before creating or updating an application.

#### Scenario: Create application with valid active customer
- **WHEN** an applicant creates an application with a `customerId` referencing an active customer
- **THEN** the system SHALL create the application successfully

#### Scenario: Create application with archived customer
- **WHEN** an applicant creates an application with a `customerId` referencing an archived customer
- **THEN** the system SHALL return HTTP 400 with message "Der ausgewählte Kunde ist archiviert"

#### Scenario: Create application with non-existent customer
- **WHEN** an applicant creates an application with a `customerId` referencing a non-existent customer
- **THEN** the system SHALL return HTTP 400 with message "Kunde nicht gefunden"

#### Scenario: Customer Service unavailable
- **WHEN** an applicant creates an application but the Customer Service is unreachable
- **THEN** the system SHALL return HTTP 503 with message "Kundendienst nicht erreichbar"

### Requirement: Application DTOs include customer information
The `ApplicationCreateDto` and `ApplicationUpdateDto` SHALL include `customerId` (int) instead of `name` (string). The `ApplicationResponse` SHALL include both `customerId` and `customerName` (resolved from Customer Service).

#### Scenario: ApplicationResponse includes customer name
- **WHEN** an application is queried
- **THEN** the response SHALL include `customerId` (int) and `customerName` (string, formatted as "FirstName LastName")

#### Scenario: ApplicationCreateDto uses customerId
- **WHEN** an applicant submits a new application
- **THEN** the request body SHALL contain `customerId` (int) instead of `name` (string)

### Requirement: Application form uses customer selection
The application form SHALL replace the name text input with a customer select dropdown showing active customers.

#### Scenario: Customer dropdown on application form
- **WHEN** an applicant opens the application form (new or edit)
- **THEN** the form SHALL display a dropdown listing all active customers formatted as "LastName, FirstName"

#### Scenario: No customers available
- **WHEN** an applicant opens the application form and no active customers exist
- **THEN** the dropdown SHALL be empty and the form SHALL show a link to create a new customer at `/customers/new`

#### Scenario: Editing existing application
- **WHEN** an applicant edits an existing draft application
- **THEN** the customer dropdown SHALL pre-select the application's current customer

### Requirement: Application validator updated for customerId
The application validators SHALL validate `customerId` instead of `name`. The `customerId` field SHALL be a required positive integer.

#### Scenario: Valid customerId
- **WHEN** an application is submitted with a positive integer `customerId`
- **THEN** validation SHALL pass (existence check happens separately in the handler)

#### Scenario: Missing customerId
- **WHEN** an application is submitted without `customerId` or with `customerId = 0`
- **THEN** validation SHALL fail with message "Bitte wählen Sie einen Kunden aus"
