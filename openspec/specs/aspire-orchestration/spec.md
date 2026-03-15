## ADDED Requirements

### Requirement: AppHost project orchestrates all backend services
The system SHALL provide an Aspire AppHost project at `src/backend/AppHost/` that orchestrates PostgreSQL, Keycloak, RiskManagement.Api, and CustomerManagement.Api. Running `dotnet run --project AppHost` SHALL start all services in the correct dependency order.

#### Scenario: Start all services with a single command
- **WHEN** a developer runs `dotnet run` in the AppHost project
- **THEN** PostgreSQL, Keycloak, RiskManagement.Api, and CustomerManagement.Api SHALL all be started
- **AND** the Aspire Dashboard SHALL be accessible

#### Scenario: Services start in dependency order
- **WHEN** the AppHost starts
- **THEN** PostgreSQL SHALL start first
- **AND** Keycloak SHALL start after PostgreSQL (if dependent) or in parallel
- **AND** both API services SHALL wait for PostgreSQL and Keycloak to be healthy before starting

### Requirement: PostgreSQL with two databases
The AppHost SHALL provision a single PostgreSQL container with two databases: `risk-management` (for RiskManagement.Api) and `customer-management` (for CustomerManagement.Api). Data SHALL be persisted via a named volume.

#### Scenario: Both databases are created and accessible
- **WHEN** the AppHost starts the PostgreSQL resource
- **THEN** both `risk-management` and `customer-management` databases SHALL exist
- **AND** each API service SHALL receive its respective connection string

#### Scenario: Data persists across restarts
- **WHEN** the AppHost is stopped and restarted
- **THEN** database data from the previous session SHALL be preserved via the data volume

### Requirement: Keycloak with realm import
The AppHost SHALL provision a Keycloak container on port 8081 with the existing realm configuration imported from `dev/keycloak/import/`.

#### Scenario: Keycloak starts with pre-configured realm
- **WHEN** the AppHost starts the Keycloak resource
- **THEN** Keycloak SHALL be accessible on port 8081
- **AND** the `risk-management` realm SHALL be available with all pre-configured clients and roles

### Requirement: Connection string injection
The AppHost SHALL inject database connection strings into each API service so that `GetConnectionString("DefaultConnection")` resolves to the Aspire-managed PostgreSQL instance.

#### Scenario: RiskManagement.Api receives correct connection string
- **WHEN** RiskManagement.Api starts under Aspire
- **THEN** `GetConnectionString("DefaultConnection")` SHALL return a connection string pointing to the `risk-management` database on the Aspire-managed PostgreSQL instance

#### Scenario: CustomerManagement.Api receives correct connection string
- **WHEN** CustomerManagement.Api starts under Aspire
- **THEN** `GetConnectionString("DefaultConnection")` SHALL return a connection string pointing to the `customer-management` database on the Aspire-managed PostgreSQL instance

### Requirement: Service-to-service URL injection
The AppHost SHALL inject service references so that RiskManagement.Api can discover CustomerManagement.Api's URL, and vice versa, without hardcoded addresses.

#### Scenario: RiskManagement.Api discovers CustomerManagement.Api
- **WHEN** RiskManagement.Api needs to call CustomerManagement.Api
- **THEN** the service URL SHALL be resolved via Aspire's service discovery mechanism
- **AND** the existing `CUSTOMER_SERVICE_URL` configuration key SHALL be populated automatically

#### Scenario: CustomerManagement.Api discovers RiskManagement.Api
- **WHEN** CustomerManagement.Api needs to call RiskManagement.Api
- **THEN** the service URL SHALL be resolved via Aspire's service discovery mechanism
- **AND** the existing `APPLICATION_SERVICE_URL` configuration key SHALL be populated automatically

### Requirement: Dual-mode compatibility
The existing docker-compose workflow SHALL continue to work unchanged. Both APIs SHALL start correctly without Aspire by using fallback values from `appsettings.Development.json`.

#### Scenario: APIs start without Aspire
- **WHEN** a developer starts PostgreSQL and Keycloak via docker-compose and runs the APIs directly with `dotnet run`
- **THEN** both APIs SHALL use connection strings and service URLs from `appsettings.Development.json`
- **AND** no Aspire-specific errors SHALL occur

### Requirement: Solution file includes AppHost
The `RiskManagementPlatform.slnx` solution file SHALL include the AppHost and ServiceDefaults projects.

#### Scenario: All projects visible in IDE
- **WHEN** a developer opens `RiskManagementPlatform.slnx`
- **THEN** the AppHost and ServiceDefaults projects SHALL appear in the solution explorer
