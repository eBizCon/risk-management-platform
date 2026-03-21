## ADDED Requirements

### Requirement: Integration event records in SharedKernel
The SharedKernel SHALL define integration event records for customer lifecycle changes. These records SHALL be used as MassTransit message contracts shared between CustomerManagement (publisher) and RiskManagement (consumer).

#### Scenario: Integration event definitions
- **WHEN** integration events are defined
- **THEN** the following records SHALL exist in `SharedKernel.IntegrationEvents`: `CustomerCreatedIntegrationEvent(int CustomerId, string FirstName, string LastName, string Status)`, `CustomerUpdatedIntegrationEvent(int CustomerId, string FirstName, string LastName, string Status)`, `CustomerActivatedIntegrationEvent(int CustomerId)`, `CustomerArchivedIntegrationEvent(int CustomerId)`, `CustomerDeletedIntegrationEvent(int CustomerId)`

#### Scenario: Integration events are simple records
- **WHEN** integration events are defined
- **THEN** they SHALL be C# record types with no behavior, suitable for serialization over RabbitMQ

### Requirement: CustomerManagement publishes integration events via MassTransit
The CustomerManagement BC SHALL publish integration events to RabbitMQ via MassTransit when customer state changes occur. Integration events SHALL be published by domain event handlers in the Infrastructure layer that react to existing domain events.

#### Scenario: Customer created triggers integration event
- **WHEN** a new customer is created in CustomerManagement (domain event `CustomerCreatedEvent` is raised)
- **THEN** the system SHALL publish a `CustomerCreatedIntegrationEvent` to RabbitMQ with `CustomerId`, `FirstName`, `LastName`, and `Status`

#### Scenario: Customer updated triggers integration event
- **WHEN** a customer is updated in CustomerManagement (domain event `CustomerUpdatedEvent` is raised)
- **THEN** the system SHALL publish a `CustomerUpdatedIntegrationEvent` to RabbitMQ with the updated `CustomerId`, `FirstName`, `LastName`, and `Status`

#### Scenario: Customer activated triggers integration event
- **WHEN** a customer is activated in CustomerManagement (domain event `CustomerActivatedEvent` is raised)
- **THEN** the system SHALL publish a `CustomerActivatedIntegrationEvent` to RabbitMQ with the `CustomerId`

#### Scenario: Customer archived triggers integration event
- **WHEN** a customer is archived in CustomerManagement (domain event `CustomerArchivedEvent` is raised)
- **THEN** the system SHALL publish a `CustomerArchivedIntegrationEvent` to RabbitMQ with the `CustomerId`

#### Scenario: Customer deleted triggers integration event
- **WHEN** a customer is deleted in CustomerManagement (domain event `CustomerDeletedEvent` is raised)
- **THEN** the system SHALL publish a `CustomerDeletedIntegrationEvent` to RabbitMQ with the `CustomerId`

### Requirement: MassTransit configuration for CustomerManagement
The CustomerManagement API SHALL be configured with MassTransit and RabbitMQ for publishing integration events. It SHALL use the same RabbitMQ instance as the RiskManagement API.

#### Scenario: MassTransit registration in CustomerManagement
- **WHEN** the CustomerManagement API starts
- **THEN** MassTransit SHALL be configured with RabbitMQ transport using kebab-case endpoint name formatter

#### Scenario: Aspire AppHost includes RabbitMQ reference for CustomerManagement
- **WHEN** the Aspire AppHost is configured
- **THEN** the `customer-api` project SHALL have a `.WithReference(rabbitmq)` and `.WaitFor(rabbitmq)` declaration
