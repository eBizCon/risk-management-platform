## MODIFIED Requirements

### Requirement: AppHost project orchestrates all backend services
The system SHALL provide an Aspire AppHost project at `src/backend/AppHost/` that orchestrates PostgreSQL, Keycloak, RabbitMQ, RiskManagement.Api, and CustomerManagement.Api. Running `dotnet run --project AppHost` SHALL start all services in the correct dependency order.

#### Scenario: Start all services with a single command
- **WHEN** a developer runs `dotnet run` in the AppHost project
- **THEN** PostgreSQL, Keycloak, RabbitMQ, RiskManagement.Api, and CustomerManagement.Api SHALL all be started
- **AND** the Aspire Dashboard SHALL be accessible

#### Scenario: Services start in dependency order
- **WHEN** the AppHost starts
- **THEN** PostgreSQL SHALL start first
- **AND** RabbitMQ SHALL start in parallel with PostgreSQL
- **AND** Keycloak SHALL start after PostgreSQL (if dependent) or in parallel
- **AND** both API services SHALL wait for PostgreSQL, Keycloak, and RabbitMQ to be healthy before starting

## ADDED Requirements

### Requirement: RabbitMQ resource in AppHost
The AppHost SHALL provision a RabbitMQ container via `builder.AddRabbitMQ("messaging")` with a data volume for message persistence. The RiskManagement.Api project SHALL receive a reference to the RabbitMQ resource via `WithReference(rabbitmq)` and SHALL wait for RabbitMQ via `WaitFor(rabbitmq)`.

#### Scenario: RabbitMQ provisioned by Aspire
- **WHEN** the AppHost starts
- **THEN** a RabbitMQ container SHALL be provisioned and accessible

#### Scenario: RiskManagement.Api connected to RabbitMQ
- **WHEN** RiskManagement.Api starts under Aspire
- **THEN** it SHALL have a connection reference to the Aspire-managed RabbitMQ instance

#### Scenario: CustomerManagement.Api does not reference RabbitMQ
- **WHEN** CustomerManagement.Api starts under Aspire
- **THEN** it SHALL NOT have a reference to RabbitMQ (only RiskManagement uses messaging in this change)
