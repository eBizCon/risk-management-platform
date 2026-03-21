## ADDED Requirements

### Requirement: MassTransit registration in RiskManagement
The RiskManagement service SHALL register MassTransit via `AddMassTransit()` in `DependencyInjection.cs`. The registration SHALL include: the `ApplicationCreationStateMachine` saga with EF Core PostgreSQL repository using the existing `ApplicationDbContext`, all saga consumers (`FetchCustomerProfileConsumer`, `PerformCreditCheckConsumer`, `FinalizeApplicationConsumer`), RabbitMQ transport configuration, and kebab-case endpoint name formatting.

#### Scenario: MassTransit registered on startup
- **WHEN** the RiskManagement.Api starts
- **THEN** MassTransit SHALL be configured with RabbitMQ transport, saga state machine, and all consumers
- **THEN** MassTransit SHALL use the existing `ApplicationDbContext` for saga persistence

#### Scenario: Endpoint names use kebab-case
- **WHEN** MassTransit configures receive endpoints
- **THEN** queue names SHALL use kebab-case formatting (e.g., `fetch-customer-profile`)

### Requirement: MassTransit retry policy
The MassTransit bus configuration SHALL include a message retry policy with intervals: 1s, 5s, 15s, 30s. This applies to all consumers. After the final retry, failed messages SHALL be moved to the error queue.

#### Scenario: Transient HTTP failure retried
- **WHEN** a consumer fails due to a transient HTTP error (e.g., CustomerManagement temporarily unavailable)
- **THEN** MassTransit SHALL retry the message after 1s, then 5s, then 15s, then 30s
- **THEN** if all retries fail, the message SHALL be moved to the error queue

#### Scenario: Permanent failure not retried indefinitely
- **WHEN** a consumer fails with a non-transient error (e.g., customer not found)
- **THEN** the consumer SHALL publish `ApplicationCreationFailed` explicitly without relying on retries

### Requirement: RabbitMQ NuGet packages
The following NuGet packages SHALL be added: `MassTransit` and `MassTransit.RabbitMQ` to `RiskManagement.Infrastructure.csproj`, `MassTransit.EntityFrameworkCore` to `RiskManagement.Infrastructure.csproj`, `Aspire.Hosting.RabbitMQ` to `AppHost.csproj`.

#### Scenario: Packages restore successfully
- **WHEN** `dotnet restore` is run on the solution
- **THEN** all MassTransit and RabbitMQ packages SHALL resolve without version conflicts

### Requirement: RabbitMQ connection via Aspire service reference
The RabbitMQ connection string SHALL be injected into RiskManagement.Api via Aspire's `WithReference(rabbitmq)` mechanism. The `AddMessaging()` extension method SHALL accept the connection string and pass it to `cfg.Host()`.

#### Scenario: RabbitMQ connection resolved via Aspire
- **WHEN** RiskManagement.Api starts under Aspire
- **THEN** the RabbitMQ connection SHALL be resolved from the Aspire-managed RabbitMQ resource

#### Scenario: Fallback connection for non-Aspire development
- **WHEN** RiskManagement.Api starts without Aspire (e.g., docker-compose)
- **THEN** the RabbitMQ connection SHALL fall back to `appsettings.Development.json` configuration

### Requirement: RabbitMQ in docker-compose
The `dev/docker-compose.yml` SHALL include a RabbitMQ service with management plugin, exposing ports 5672 (AMQP) and 15672 (management UI), with a data volume for message persistence.

#### Scenario: RabbitMQ starts with docker-compose
- **WHEN** `docker-compose up` is run in the dev directory
- **THEN** a RabbitMQ container SHALL be started and accessible on port 5672
- **THEN** the RabbitMQ management UI SHALL be accessible on port 15672
