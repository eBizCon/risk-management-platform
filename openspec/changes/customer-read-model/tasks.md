## 1. Integration Events in SharedKernel

- [x] 1.1 Create `SharedKernel/IntegrationEvents/` directory and define integration event records: `CustomerCreatedIntegrationEvent`, `CustomerUpdatedIntegrationEvent`, `CustomerActivatedIntegrationEvent`, `CustomerArchivedIntegrationEvent`, `CustomerDeletedIntegrationEvent`

## 2. CustomerManagement: MassTransit Setup & Event Publishing

- [x] 2.1 Add MassTransit and RabbitMQ NuGet packages to `CustomerManagement.Infrastructure`
- [x] 2.2 Add `AddMessaging(string connectionString)` method in `CustomerManagement.Infrastructure.DependencyInjection` configuring MassTransit with RabbitMQ (publish-only, no consumers needed)
- [x] 2.3 Register messaging in `CustomerManagement.Api/Program.cs` using the RabbitMQ connection string
- [x] 2.4 Create domain event handlers in `CustomerManagement.Infrastructure/IntegrationEventPublishers/` that react to existing domain events (`CustomerCreatedEvent`, `CustomerUpdatedEvent`, `CustomerActivatedEvent`, `CustomerArchivedEvent`, `CustomerDeletedEvent`) and publish the corresponding integration events via `IPublishEndpoint`
- [x] 2.5 Update Aspire AppHost: add `.WithReference(rabbitmq)` and `.WaitFor(rabbitmq)` to the `customer-api` project

## 3. RiskManagement: CustomerReadModel Entity & Persistence

- [x] 3.1 Create `CustomerReadModel` entity in `RiskManagement.Domain/ReadModels/` with properties: `CustomerId` (int, PK), `FirstName`, `LastName`, `Status`, `LastUpdatedAt`
- [x] 3.2 Create `CustomerReadModelConfiguration` in `RiskManagement.Infrastructure/Persistence/Configurations/` mapping to `customer_read_models` table
- [x] 3.3 Add `DbSet<CustomerReadModel>` to `ApplicationDbContext`
- [x] 3.4 Create EF Core migration `AddCustomerReadModel`

## 4. RiskManagement: MassTransit Consumers for Integration Events

- [x] 4.1 Create `CustomerCreatedConsumer` in `RiskManagement.Infrastructure/Sagas/Consumers/` that inserts/upserts `CustomerReadModel` on `CustomerCreatedIntegrationEvent`
- [x] 4.2 Create `CustomerUpdatedConsumer` that updates `CustomerReadModel` on `CustomerUpdatedIntegrationEvent`
- [x] 4.3 Create `CustomerStatusChangedConsumer` that handles both `CustomerActivatedIntegrationEvent` and `CustomerArchivedIntegrationEvent` by updating the status
- [x] 4.4 Create `CustomerDeletedConsumer` that removes `CustomerReadModel` on `CustomerDeletedIntegrationEvent`
- [x] 4.5 Register all new consumers in `RiskManagement.Infrastructure.DependencyInjection.AddMessaging()`

## 5. RiskManagement: API Endpoint for Customer List

- [x] 5.1 Create `GetActiveCustomersQuery` and `GetActiveCustomersHandler` that queries active `CustomerReadModel` entries ordered by LastName, FirstName
- [x] 5.2 Add `GET /api/applications/customers` endpoint to the Applications controller (or a new controller) returning the active customers

## 6. RiskManagement: Initial Sync on Startup

- [x] 6.1 Create `CustomerReadModelSyncService` (hosted service or startup task) that checks if `customer_read_models` table is empty and fetches all customers from `GET /api/internal/customers` on the CustomerManagement API
- [x] 6.2 Add `GET /api/internal/customers` endpoint to CustomerManagement API returning all customers (id, firstName, lastName, status)
- [x] 6.3 Register the sync service in `RiskManagement.Api/Program.cs`

## 7. Frontend: Switch Customer Dropdown Source

- [x] 7.1 Update `ApplicationForm.svelte` to fetch from `/api/applications/customers` instead of `/api/customers/active`
- [x] 7.2 Verify the BFF proxy routes `/api/applications/*` to the RiskManagement API (already configured in `hooks.server.ts`)

## 8. Tests

- [x] 8.1 Write unit tests for integration event publisher handlers in CustomerManagement
- [x] 8.2 Write unit tests for CustomerReadModel consumers in RiskManagement (create, update, status change, delete, idempotent upsert)
- [x] 8.3 Write unit test for `GetActiveCustomersHandler`
- [x] 8.4 Verify existing backend tests still pass (`dotnet test`)
