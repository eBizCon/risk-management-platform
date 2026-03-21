## Context

The RiskManagement BC currently fetches the customer list directly from the CustomerManagement API via the BFF proxy (`/api/customers/active` → CustomerManagement API). This creates a synchronous runtime dependency: if CustomerManagement is down, the application form cannot load customers and no credit applications can be created.

The Saga already uses RabbitMQ (via MassTransit) for asynchronous processing. CustomerManagement currently has no MassTransit integration — it only exposes synchronous HTTP APIs.

Existing domain events in CustomerManagement (`CustomerCreatedEvent`, `CustomerUpdatedEvent`, `CustomerActivatedEvent`, `CustomerArchivedEvent`, `CustomerDeletedEvent`) are in-process domain events dispatched via the `IDispatcher`. They are not published to RabbitMQ.

## Goals / Non-Goals

**Goals:**
- Eliminate the runtime dependency on CustomerManagement API for the customer dropdown in the application form
- Introduce event-driven data synchronization between CustomerManagement and RiskManagement via RabbitMQ
- Maintain a local `CustomerReadModel` in the RiskManagement database for customer list queries
- Keep the read model minimal — only data needed for selection (id, name, status)

**Non-Goals:**
- Replacing the synchronous `ICustomerProfileService` HTTP call in the Saga's `FetchCustomerProfileConsumer` — detailed customer data (employment status, credit report) for scoring still requires the synchronous call
- Full CQRS/event sourcing — this is a simple projected read model, not an event store
- Real-time consistency — eventual consistency (seconds delay) is acceptable for the customer dropdown
- Replicating all customer fields — only id, first name, last name, and status are needed

## Decisions

### 1. Integration Events via MassTransit (not HTTP polling or shared DB)

**Decision:** Publish integration events from CustomerManagement to RabbitMQ using MassTransit; consume them in RiskManagement.

**Alternatives considered:**
- **HTTP polling**: RiskManagement periodically fetches the customer list — adds load, latency, and still fails during outages
- **Shared database**: Both BCs read from the same table — violates BC autonomy and creates tight coupling
- **Change Data Capture (CDC)**: Overkill for this use case, adds infrastructure complexity

**Rationale:** MassTransit is already in use for the Saga. Adding it to CustomerManagement is the natural next step. Events are the canonical DDD way to synchronize read models across bounded contexts.

### 2. Separate Integration Events (not reusing Domain Events)

**Decision:** Define new integration event records in a shared `SharedKernel.IntegrationEvents` namespace, separate from the existing in-process domain events.

**Alternatives considered:**
- **Reuse existing domain events directly**: Would leak domain internals across BC boundaries
- **Publish domain events to RabbitMQ via MassTransit**: Would tightly couple the internal domain model to the message contract

**Rationale:** Integration events are a public contract between bounded contexts. They should be stable, versioned independently, and contain only the data the consumer needs. Domain events are internal implementation details.

### 3. CustomerReadModel as simple EF Core entity (not Saga state or cache)

**Decision:** Store the read model as a standard EF Core entity in the RiskManagement `ApplicationDbContext` with its own `customer_read_models` table.

**Alternatives considered:**
- **In-memory cache**: Lost on restart, requires warm-up, no persistence
- **Separate read-side DbContext**: Overengineered for a single table

**Rationale:** Simple, persistent, queryable, and consistent with the existing architecture. The table is small and rarely changes.

### 4. Publish integration events after domain event handling (not in aggregate)

**Decision:** Publish integration events from dedicated `IDomainEventHandler<>` implementations in CustomerManagement.Infrastructure that react to existing domain events and forward them as integration events via `IPublishEndpoint`.

**Rationale:** This keeps the domain layer clean. The aggregate raises domain events as before; infrastructure handlers translate them to integration events. No changes to the domain model.

### 5. New RiskManagement API endpoint for customer list

**Decision:** Add `GET /api/applications/customers` to the RiskManagement API that queries the local `CustomerReadModel` table. The frontend fetches from this endpoint instead of proxying to CustomerManagement.

**Alternatives considered:**
- **Keep BFF proxy but point to RiskManagement**: Would work but the endpoint name `/api/customers/active` is misleading if it doesn't go to CustomerManagement
- **New BFF route**: Unnecessary indirection since the RiskManagement API can serve it directly

**Rationale:** Clear ownership — the RiskManagement API owns the data it serves to the application form.

## Risks / Trade-offs

- **Eventual consistency** → The customer dropdown may show stale data for a few seconds after a customer is created/updated. Mitigation: This is acceptable for a dropdown; the Saga still validates the customer synchronously.
- **Initial sync on empty read model** → When first deployed, the read model table is empty. Mitigation: Add a one-time sync endpoint or seed command that fetches all active customers from CustomerManagement API to bootstrap the read model.
- **Message ordering** → MassTransit does not guarantee strict ordering. Mitigation: Use `CustomerId` as the partition key; for the read model (last-write-wins on name/status), ordering is not critical.
- **CustomerManagement needs RabbitMQ dependency** → Adds infrastructure complexity. Mitigation: MassTransit setup is well-understood from the RiskManagement implementation; the pattern is identical.

## Open Questions

- Should the initial sync be a CLI command, an API endpoint, or automatic on startup? (Recommendation: automatic on startup if the table is empty)
