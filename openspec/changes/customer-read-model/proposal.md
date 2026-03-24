## Why

The RiskManagement bounded context has a runtime dependency on the CustomerManagement API for displaying the customer dropdown in the application form. If the CustomerManagement API is unavailable, applicants cannot create credit applications — even though the customer data is only needed for selection and display purposes. This coupling contradicts the autonomy principle of bounded contexts and reduces overall system resilience.

## What Changes

- Introduce a `CustomerReadModel` in the RiskManagement BC that stores a projected subset of customer data (id, name, status) synchronized via domain events over RabbitMQ
- CustomerManagement publishes integration events (`CustomerCreated`, `CustomerUpdated`, `CustomerActivated`, `CustomerArchived`, `CustomerDeleted`) to RabbitMQ when customer state changes
- RiskManagement consumes these events via MassTransit consumers and maintains a local `customer_read_models` table
- The application form fetches the customer list from the RiskManagement API instead of the CustomerManagement API, removing the cross-BC runtime dependency
- The BFF proxy route for `/api/customers/active` is replaced with a RiskManagement API endpoint
- The existing synchronous `ICustomerProfileService` HTTP call in the Saga's `FetchCustomerProfileConsumer` remains unchanged — it fetches detailed customer data (employment status, credit report) needed for scoring, which is not part of the read model

## Capabilities

### New Capabilities
- `customer-read-model`: Event-driven projection of customer data in the RiskManagement BC for autonomous customer list queries
- `customer-integration-events`: Publishing of customer lifecycle integration events from CustomerManagement via RabbitMQ

### Modified Capabilities
- `service-communication`: The frontend BFF no longer proxies `/api/customers/active` to the CustomerManagement API; it queries the RiskManagement API instead

## Impact

- **Backend (RiskManagement)**: New `CustomerReadModel` entity, EF Core configuration, migration, MassTransit consumers, new API endpoint `GET /api/applications/customers`
- **Backend (CustomerManagement)**: MassTransit integration for publishing integration events on customer create/update/activate/archive/delete
- **Frontend**: `ApplicationForm.svelte` fetch URL changes from `/api/customers/active` to RiskManagement-hosted endpoint
- **Infrastructure**: CustomerManagement API needs RabbitMQ reference in Aspire AppHost
- **Database**: New `customer_read_models` table in the RiskManagement database
