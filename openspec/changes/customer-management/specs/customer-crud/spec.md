## ADDED Requirements

### Requirement: Customer aggregate with structured data
The system SHALL provide a Customer aggregate with the following fields: FirstName (string, 2-50 chars, required), LastName (string, 2-50 chars, required), Email (EmailAddress value object, optional), Phone (PhoneNumber value object, required), DateOfBirth (DateOnly, required), Address (Address value object, required), Status (CustomerStatus: Active/Archived), CreatedBy (EmailAddress), CreatedAt (DateTime), UpdatedAt (DateTime?).

#### Scenario: Customer has all required fields
- **WHEN** a Customer is persisted
- **THEN** it SHALL have non-null values for FirstName, LastName, Phone, DateOfBirth, Address, Status, CreatedBy, CreatedAt

#### Scenario: CustomerId is strongly typed
- **WHEN** a Customer is created
- **THEN** its Id SHALL be of type `CustomerId` (int, auto-generated)

### Requirement: Create customer
The system SHALL allow users with the `applicant` role to create a new customer with all required fields. A newly created customer SHALL have status `Active`.

#### Scenario: Successful customer creation
- **WHEN** an applicant submits a valid customer creation request via `POST /api/customers`
- **THEN** the system SHALL create the customer with status `Active`, set `CreatedBy` to the current user's email, set `CreatedAt` to the current UTC time, and return the created customer with HTTP 201

#### Scenario: Validation failure on creation
- **WHEN** an applicant submits a customer creation request with invalid data (e.g., FirstName shorter than 2 characters, missing Address, invalid PhoneNumber)
- **THEN** the system SHALL return HTTP 400 with field-level validation errors

### Requirement: Read customer
The system SHALL allow users with the `applicant` role to retrieve customers.

#### Scenario: Get customer list
- **WHEN** an applicant requests `GET /api/customers`
- **THEN** the system SHALL return all customers with status `Active` (archived customers are excluded by default)

#### Scenario: Get customer list including archived
- **WHEN** an applicant requests `GET /api/customers?includeArchived=true`
- **THEN** the system SHALL return all customers regardless of status

#### Scenario: Get single customer
- **WHEN** an applicant requests `GET /api/customers/{id}`
- **THEN** the system SHALL return the customer details including all fields

#### Scenario: Customer not found
- **WHEN** an applicant requests `GET /api/customers/{id}` with a non-existent ID
- **THEN** the system SHALL return HTTP 404

### Requirement: Update customer
The system SHALL allow users with the `applicant` role to update an existing customer's data. Only active customers can be updated.

#### Scenario: Successful update
- **WHEN** an applicant submits a valid update request via `PUT /api/customers/{id}` for an active customer
- **THEN** the system SHALL update the customer fields and set `UpdatedAt` to the current UTC time

#### Scenario: Update archived customer
- **WHEN** an applicant attempts to update a customer with status `Archived`
- **THEN** the system SHALL return HTTP 400 with message "Archivierte Kunden können nicht bearbeitet werden"

#### Scenario: Validation failure on update
- **WHEN** an applicant submits an update request with invalid data
- **THEN** the system SHALL return HTTP 400 with field-level validation errors

### Requirement: Delete customer
The system SHALL allow users with the `applicant` role to permanently delete a customer, but only if the customer has no linked applications.

#### Scenario: Successful deletion
- **WHEN** an applicant requests `DELETE /api/customers/{id}` for a customer with zero linked applications
- **THEN** the system SHALL permanently delete the customer and return HTTP 200

#### Scenario: Delete customer with applications
- **WHEN** an applicant requests `DELETE /api/customers/{id}` for a customer that has one or more linked applications
- **THEN** the system SHALL return HTTP 409 with message "Kunde kann nicht gelöscht werden, da Anträge vorhanden sind"

#### Scenario: Delete non-existent customer
- **WHEN** an applicant requests `DELETE /api/customers/{id}` with a non-existent ID
- **THEN** the system SHALL return HTTP 404

### Requirement: Archive customer
The system SHALL allow users with the `applicant` role to archive an active customer. Archived customers SHALL be excluded from default list queries and SHALL NOT be selectable for new applications.

#### Scenario: Successful archival
- **WHEN** an applicant requests `POST /api/customers/{id}/archive` for an active customer
- **THEN** the system SHALL set the customer status to `Archived` and return the updated customer

#### Scenario: Archive already archived customer
- **WHEN** an applicant requests `POST /api/customers/{id}/archive` for a customer with status `Archived`
- **THEN** the system SHALL return HTTP 400 with message "Kunde ist bereits archiviert"

### Requirement: Activate customer
The system SHALL allow users with the `applicant` role to reactivate an archived customer.

#### Scenario: Successful activation
- **WHEN** an applicant requests `POST /api/customers/{id}/activate` for an archived customer
- **THEN** the system SHALL set the customer status to `Active` and return the updated customer

#### Scenario: Activate already active customer
- **WHEN** an applicant requests `POST /api/customers/{id}/activate` for a customer with status `Active`
- **THEN** the system SHALL return HTTP 400 with message "Kunde ist bereits aktiv"

### Requirement: Address value object
The system SHALL represent customer addresses as an `Address` value object with fields: Street (string, required), HouseNumber (string, required), PostalCode (string, required), City (string, required), Country (string, required, default "DE").

#### Scenario: Valid address
- **WHEN** an Address is created with all required fields
- **THEN** it SHALL be a valid value object with equality based on all fields

#### Scenario: Invalid address
- **WHEN** an Address is created with any required field empty or null
- **THEN** the system SHALL reject it with a validation error

### Requirement: PhoneNumber value object
The system SHALL represent phone numbers as a `PhoneNumber` value object that validates the format.

#### Scenario: Valid phone number
- **WHEN** a PhoneNumber is created with a valid format (e.g., "+49 123 4567890", "0123 4567890")
- **THEN** it SHALL be stored as a normalized string

#### Scenario: Invalid phone number
- **WHEN** a PhoneNumber is created with an invalid format (e.g., empty string, "abc")
- **THEN** the system SHALL reject it with a validation error

### Requirement: Customer management UI
The system SHALL provide a customer management UI accessible to the `applicant` role with list, create, detail, and edit views.

#### Scenario: Customer list page
- **WHEN** an applicant navigates to `/customers`
- **THEN** the system SHALL display a list of active customers with table/card toggle, showing FirstName, LastName, Email, Phone, and City

#### Scenario: Create customer page
- **WHEN** an applicant navigates to `/customers/new`
- **THEN** the system SHALL display a form with all customer fields and a save button

#### Scenario: Customer detail page
- **WHEN** an applicant navigates to `/customers/{id}`
- **THEN** the system SHALL display all customer details and action buttons for edit, archive/activate, and delete (if no applications)

#### Scenario: Edit customer page
- **WHEN** an applicant navigates to `/customers/{id}/edit`
- **THEN** the system SHALL display a pre-filled form with the customer's current data

### Requirement: Customer database schema
The system SHALL store customers in the `customer` PostgreSQL schema using table `customer.customers`.

#### Scenario: Schema isolation
- **WHEN** the Customer Service starts
- **THEN** it SHALL use the `customer` schema for all its database operations, separate from the `public` schema used by the Application Service
