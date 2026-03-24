## ADDED Requirements

### Requirement: CustomerReadModel entity in RiskManagement BC
The RiskManagement BC SHALL maintain a `CustomerReadModel` entity with fields: `CustomerId` (int, primary key, not auto-generated), `FirstName` (string, required), `LastName` (string, required), `Status` (string, required: "active" or "archived"), `LastUpdatedAt` (DateTime, required). The entity SHALL be stored in the `customer_read_models` table in the RiskManagement database.

#### Scenario: CustomerReadModel schema
- **WHEN** the RiskManagement database is migrated
- **THEN** the `customer_read_models` table SHALL exist with columns `customer_id` (int, PK), `first_name` (varchar), `last_name` (varchar), `status` (varchar), `last_updated_at` (timestamp)

#### Scenario: CustomerReadModel is not an aggregate
- **WHEN** the `CustomerReadModel` is defined
- **THEN** it SHALL be a simple entity (not an aggregate root) without domain events or business logic

### Requirement: MassTransit consumers for customer integration events
The RiskManagement BC SHALL register MassTransit consumers that handle incoming customer integration events and upsert/delete the `CustomerReadModel` accordingly.

#### Scenario: CustomerCreated event consumed
- **WHEN** a `CustomerCreatedIntegrationEvent` is received with `CustomerId=1`, `FirstName="Max"`, `LastName="Mustermann"`, `Status="active"`
- **THEN** the system SHALL insert a new `CustomerReadModel` with those values

#### Scenario: CustomerUpdated event consumed
- **WHEN** a `CustomerUpdatedIntegrationEvent` is received with `CustomerId=1`, `FirstName="Maximilian"`, `LastName="Mustermann"`
- **THEN** the system SHALL update the existing `CustomerReadModel` with `CustomerId=1` to reflect the new name

#### Scenario: CustomerActivated event consumed
- **WHEN** a `CustomerActivatedIntegrationEvent` is received with `CustomerId=1`
- **THEN** the system SHALL update the `CustomerReadModel` with `CustomerId=1` to `Status="active"`

#### Scenario: CustomerArchived event consumed
- **WHEN** a `CustomerArchivedIntegrationEvent` is received with `CustomerId=1`
- **THEN** the system SHALL update the `CustomerReadModel` with `CustomerId=1` to `Status="archived"`

#### Scenario: CustomerDeleted event consumed
- **WHEN** a `CustomerDeletedIntegrationEvent` is received with `CustomerId=1`
- **THEN** the system SHALL delete the `CustomerReadModel` with `CustomerId=1`

#### Scenario: Idempotent upsert
- **WHEN** a `CustomerCreatedIntegrationEvent` is received for a `CustomerId` that already exists in the read model
- **THEN** the system SHALL update the existing record instead of throwing a duplicate key error

### Requirement: Active customers API endpoint in RiskManagement
The RiskManagement API SHALL expose `GET /api/applications/customers` that returns all active customers from the local `CustomerReadModel` table, ordered by last name then first name.

#### Scenario: List active customers
- **WHEN** an authenticated applicant requests `GET /api/applications/customers`
- **THEN** the system SHALL return HTTP 200 with a JSON array of active customers containing `id`, `firstName`, `lastName`

#### Scenario: Archived customers excluded
- **WHEN** the `CustomerReadModel` table contains customers with `Status="active"` and `Status="archived"`
- **THEN** `GET /api/applications/customers` SHALL return only customers with `Status="active"`

#### Scenario: Empty read model
- **WHEN** the `CustomerReadModel` table is empty
- **THEN** `GET /api/applications/customers` SHALL return HTTP 200 with an empty array

### Requirement: Frontend fetches customers from RiskManagement API
The `ApplicationForm.svelte` component SHALL fetch the customer list from `/api/applications/customers` (served by the RiskManagement API) instead of `/api/customers/active` (served by the CustomerManagement API).

#### Scenario: Application form loads customer dropdown
- **WHEN** an applicant opens the application form
- **THEN** the form SHALL fetch customers from `/api/applications/customers`
- **THEN** the dropdown SHALL display `lastName, firstName` for each customer

#### Scenario: CustomerManagement API unavailable
- **WHEN** the CustomerManagement API is down but the RiskManagement API is running
- **THEN** the application form SHALL still display the customer dropdown with data from the local read model

### Requirement: Initial sync on startup
The RiskManagement API SHALL perform an initial synchronization of customer data on startup if the `CustomerReadModel` table is empty. It SHALL call the CustomerManagement API's `GET /api/customers` endpoint to fetch all customers and populate the read model.

#### Scenario: First startup with empty read model
- **WHEN** the RiskManagement API starts and the `customer_read_models` table is empty
- **THEN** the system SHALL fetch all customers from the CustomerManagement API and insert them into the read model

#### Scenario: Subsequent startup with existing data
- **WHEN** the RiskManagement API starts and the `customer_read_models` table already has data
- **THEN** the system SHALL NOT perform the initial sync

#### Scenario: CustomerManagement API unavailable on startup
- **WHEN** the RiskManagement API starts with an empty read model but the CustomerManagement API is unreachable
- **THEN** the system SHALL log a warning and continue startup without failing
